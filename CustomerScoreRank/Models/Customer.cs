using System.ComponentModel.DataAnnotations;

namespace CustomerScoreRank.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public decimal Score { get; set; }
    }
}
