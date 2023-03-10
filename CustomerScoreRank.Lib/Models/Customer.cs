namespace CustomerScoreRank.Lib.Models
{
    public class Customer : IComparable<Customer>
    {
        public long Id { get; set; }
        public decimal Score { get; set; }

        public int CompareTo(Customer? other)
        {
            return other == null || Score > other.Score || (Score == other.Score && Id < other.Id) ? -1 : 1;
        }
    }
}
