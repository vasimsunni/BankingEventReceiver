namespace BankingApi.EventReceiver
{
    public class TransactionHistory
    {
        public Guid Id { get; set; }
        public Guid BankAccountId { get; set; }
        public Guid BankAccountTransactionId { get; set; }
        public string Type { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal UpdatedBalance { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
