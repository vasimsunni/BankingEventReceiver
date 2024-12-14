using Microsoft.Extensions.Logging;

namespace BankingApi.EventReceiver
{
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly BankingApiDbContext _bankingApiDbContext;
        private readonly ILogger<MessageWorker> _logger;

        public TransactionHistoryService(BankingApiDbContext bankingApiDbContext, ILogger<MessageWorker> logger)
        {
            _bankingApiDbContext = bankingApiDbContext;
            _logger = logger;
        }

        public async Task LogTransactionHistory(BankAccount account,
                                                BankAccountTransaction transaction,
                                                string type,
                                                decimal previousBalance,
                                                decimal updatedBalance,
                                                CancellationToken cancellationToken)
        {
            var transactionHistory = new TransactionHistory
            {
                Id = Guid.NewGuid(),
                BankAccountId = account.Id,
                BankAccountTransactionId = transaction.Id,
                Type = type,
                PreviousBalance = previousBalance,
                UpdatedBalance = updatedBalance,
                Amount = transaction.Amount,
                TransactionDate = DateTime.UtcNow
            };

            await _bankingApiDbContext.TransactionHistories.AddAsync(transactionHistory, cancellationToken);
            await _bankingApiDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
