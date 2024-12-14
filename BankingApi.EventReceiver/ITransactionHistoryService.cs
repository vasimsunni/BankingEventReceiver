namespace BankingApi.EventReceiver
{
    public interface ITransactionHistoryService
    {
        Task LogTransactionHistory(BankAccount account,
                                          BankAccountTransaction transaction,
                                          string type,
                                          decimal previousBalance,
                                          decimal updatedBalance,
                                          CancellationToken cancellationToken);
    }
}
