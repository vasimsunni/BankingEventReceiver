using Microsoft.Extensions.Logging;
using Polly;
using System.Text.Json;

namespace BankingApi.EventReceiver
{
    public class MessageWorker
    {
        private readonly IServiceBusReceiver _serviceBusReceiver;
        private readonly ITransactionHistoryService _transactionHistoryService;
        private readonly BankingApiDbContext _bankingApiDbContext;
        private readonly ILogger<MessageWorker> _logger;

        public MessageWorker(IServiceBusReceiver serviceBusReceiver,
                             ITransactionHistoryService transactionHistoryService,
                             BankingApiDbContext bankingApiDbContext,
                             ILogger<MessageWorker> logger)
        {
            _serviceBusReceiver = serviceBusReceiver;
            _transactionHistoryService = transactionHistoryService;
            _bankingApiDbContext = bankingApiDbContext;
            _logger = logger;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            // Implement logic to listen to messages here
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _serviceBusReceiver.Peek();

                if (message == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    continue;
                }

                _logger.LogInformation($"Transaction Request received with message id: {message.Id} " +
                            $"with request data: {message.MessageBody}");

                await ProcessMessage(message, cancellationToken);
            }
        }

        private async Task ProcessMessage(EventMessage message, CancellationToken cancellationToken)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(new[]
                    { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(125) });

            await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<BankAccountTransaction>(message.MessageBody);

                    if (eventData == null)
                    {
                        await _serviceBusReceiver.MoveToDeadLetter(message);
                        return;
                    }

                    switch (eventData.MessageType)
                    {
                        case "Credit":
                            await ValidateAndCredit(eventData, cancellationToken);
                            await _serviceBusReceiver.Complete(message);

                            _logger.LogInformation("Transaction Request completed successfully with message id:" 
                                + message.Id);
                            break;

                        case "Debit":
                            await ValidateAndDebit(eventData, cancellationToken);
                            await _serviceBusReceiver.Complete(message);

                            _logger.LogInformation("Transaction Request completed successfully with message id:" 
                                + message.Id);
                            break;

                        default:
                            await _serviceBusReceiver.MoveToDeadLetter(message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (message.ProcessingCount >= 3)
                    {
                        await _serviceBusReceiver.MoveToDeadLetter(message);

                        _logger.LogError($"Transaction Request failed with message id: {message.Id} " +
                            $"with request data: {message.MessageBody} Error Message: {ex.Message} Error: {ex.ToString()}");
                    }
                    else
                    {
                        await _serviceBusReceiver.Abandon(message);

                        _logger.LogWarning($"Transaction Request failed with message id: {message.Id} " +
                            $"with retry ({message.ProcessingCount})");
                    }
                }
            });
        }


        private async Task ValidateAndCredit(BankAccountTransaction transaction, 
                                             CancellationToken cancellationToken)
        {
            var account = await _bankingApiDbContext.BankAccounts.FindAsync(transaction.BankAccountId);
            if (account == null) 
                throw new Exception("Bank Account does not exists");

            decimal previousBalance = account.Balance;
            account.Balance += transaction.Amount;
            await _bankingApiDbContext.SaveChangesAsync(cancellationToken);

            // Log the transaction history
            await _transactionHistoryService.LogTransactionHistory(account, transaction, "Credit", previousBalance, account.Balance, cancellationToken);
        }

        private async Task ValidateAndDebit(BankAccountTransaction transaction, 
                                            CancellationToken cancellationToken)
        {
            var account = await _bankingApiDbContext.BankAccounts.FindAsync(transaction.BankAccountId);
            if (account == null) 
                throw new Exception("Bank Account does not exists");

            if (account.Balance < transaction.Amount) 
                throw new Exception("Insufficient balance in bank account");

            decimal previousBalance = account.Balance;
            account.Balance -= transaction.Amount;
            await _bankingApiDbContext.SaveChangesAsync(cancellationToken);

            // Log the transaction history
            await _transactionHistoryService.LogTransactionHistory(account, transaction, "Debit", previousBalance, account.Balance, cancellationToken);
        }
    }
}
