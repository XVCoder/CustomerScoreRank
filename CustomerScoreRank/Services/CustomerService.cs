using CustomerScoreRank.Models;

namespace CustomerScoreRank.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly List<Customer> _customers;

        public CustomerService()
        {
            _customers = new List<Customer>();
        }

        public async Task<decimal> UpdateScore(long customerId, decimal score)
        {
            decimal finalScore;
            lock (_customers)
            {
                if (_customers.Any(x => x.Id == customerId))
                {// Update
                    var customer = _customers.First(x => x.Id == customerId);
                    customer.Score += score;
                    finalScore = customer.Score;
                }
                else
                {// Add
                    var customer = new Customer { Id = customerId, Score = score };
                    _customers.Add(customer);
                    finalScore = customer.Score;
                }
            }
            return await Task.FromResult(finalScore);
        }

        public async Task<List<CustomerScoreRankInfo>> GetCustomersByRank(int start, int end)
        {
            var matchedCustomers = _customers
                .OrderByDescending(x => x.Score).ThenBy(x => x.Id)
                .Skip(start - 1)
                .Take(end - start + 1)
                .ToList();

            var result = new List<CustomerScoreRankInfo>();
            Parallel.For(0, matchedCustomers.Count, i =>
            {
                result.Add(new CustomerScoreRankInfo
                {
                    CustomerId = matchedCustomers[i].Id,
                    Score = matchedCustomers[i].Score,
                    Rank = start + i
                });
            });

            return await Task.FromResult(result.OrderBy(x => x.Rank).ToList());
        }

        public async Task<List<CustomerScoreRankInfo>> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0)
        {
            var indexOfTargetCustomer = _customers
                .OrderByDescending(x => x.Score).ThenBy(x => x.Id)
                .ToList()
                .FindIndex(x => x.Id == customerId);

            high = indexOfTargetCustomer - high < 0 ? 0 : high;
            low = indexOfTargetCustomer + low > _customers.Count - 1 ? _customers.Count - 1 : low;

            return await GetCustomersByRank(indexOfTargetCustomer - high + 1, indexOfTargetCustomer + low + 1);
        }

        public async Task<bool> IsCustomerExist(long customerId)
        {
            return await Task.FromResult(_customers.Any(x => x.Id == customerId));
        }
    }
}
