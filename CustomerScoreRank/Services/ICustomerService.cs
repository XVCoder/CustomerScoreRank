using CustomerScoreRank.Models;

namespace CustomerScoreRank.Services
{
    public interface ICustomerService
    {
        Task<decimal> UpdateScore(long customerId, decimal score);
        Task<List<CustomerScoreRankInfo>> GetCustomersByRank(int start, int end);
        Task<List<CustomerScoreRankInfo>> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0);
        Task<bool> IsCustomerExist(long customerId);
    }
}
