namespace BankingApi.EventReceiver
{
    public class BankAccountTransaction
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; } = string.Empty; // Credit or Debit
        public Guid BankAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
