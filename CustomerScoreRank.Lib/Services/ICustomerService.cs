using CustomerScoreRank.Lib.Models;

namespace CustomerScoreRank.Lib.Services
{
    public interface ICustomerService
    {
        decimal UpdateScore(long customerId, decimal score);
        List<CustomerScoreRankInfo> GetCustomersByRank(int start, int end);
        List<CustomerScoreRankInfo> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0);
        bool IsCustomerExist(long customerId);
        bool IsCustomerListEmpty();
    }
}
