using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankingApi.EventReceiver
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create the host builder
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Add the appsettings.json file to configuration
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Get the connection string from configuration
                    var connectionString = hostContext.Configuration.GetConnectionString("BankingApiConnection");

                    // Register DbContext with the connection string
                    services.AddDbContext<BankingApiDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    // Register application services
                    services.AddScoped<ITransactionHistoryService, TransactionHistoryService>();
                    services.AddScoped<IServiceBusReceiver, ServiceBusReceiver>();
                    services.AddScoped<MessageWorker>();

                    // Register hosted service that will start the MessageWorker
                    services.AddHostedService<Worker>();
                })
                .ConfigureLogging(logging =>
                {
                    // Set up logging configuration (optional)
                    // Logging should be done in the distribute system e.g, Postgres, Redis, MongoDb, CosmosDb etc
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            // Run the application
            await host.RunAsync();
        }
    }


    // Worker class for starting the MessageWorker in the background
    public class Worker : BackgroundService
    {
        private readonly MessageWorker _messageWorker;

        public Worker(MessageWorker messageWorker)
        {
            _messageWorker = messageWorker;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Start the message processing in the background
            await _messageWorker.Start(cancellationToken);
        }
    }
}
