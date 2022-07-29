using CustomerScoreRank.Models;
using System.Collections.Concurrent;

namespace CustomerScoreRank.Services
{
    public class CustomerService : ICustomerService
    {
        private static readonly List<Customer> _customers = new();

        public CustomerService()
        {
        }

        /// <summary>
        /// 更新顾客的分数（累加）
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <param name="score">分数</param>
        /// <returns></returns>
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

        /// <summary>
        /// 根据排名获得顾客列表
        /// </summary>
        /// <param name="start">起始排名</param>
        /// <param name="end">结束排名</param>
        /// <returns></returns>
        public async Task<List<CustomerScoreRankInfo>> GetCustomersByRank(int start, int end)
        {
            List<Customer> matchedCustomers = GetCustomersFromOrderedList(start - 1, end - start + 1);

            var resultBag = new ConcurrentBag<CustomerScoreRankInfo>();
            Parallel.For(0, matchedCustomers.Count, i =>
            {
                resultBag.Add(new CustomerScoreRankInfo
                {
                    CustomerId = matchedCustomers[i].Id,
                    Score = matchedCustomers[i].Score,
                    Rank = start + i
                });
            });

            return await Task.FromResult(resultBag.OrderBy(x => x.Rank).ToList());
        }

        /// <summary>
        /// 从以排序的顾客列表中获取顾客列表
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="tackCount">获取数量</param>
        /// <returns></returns>
        private List<Customer> GetCustomersFromOrderedList(int offset, int tackCount)
        {
            return _customers
                .OrderByDescending(x => x.Score).ThenBy(x => x.Id)
                .Skip(offset)
                .Take(tackCount)
                .ToList();
        }

        /// <summary>
        /// 根据顾客Id获取顾客所处排名前[high]、后[low]个顾客的数据
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <param name="high">排在目标顾客前面的顾客个数</param>
        /// <param name="low">排在目标顾客后面的顾客个数</param>
        /// <returns></returns>
        public async Task<List<CustomerScoreRankInfo>> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0)
        {
            // 获取目标顾客在分数排名中的位置（列表索引）
            var indexOfTargetCustomer = GetCustomersFromOrderedList(0, _customers.Count).FindIndex(x => x.Id == customerId);

            // 防止索引溢出
            high = indexOfTargetCustomer - high < 0 ? 0 : high;
            low = indexOfTargetCustomer + low > _customers.Count - 1 ? _customers.Count - 1 : low;

            return await GetCustomersByRank(indexOfTargetCustomer - high + 1, indexOfTargetCustomer + low + 1);
        }

        /// <summary>
        /// 根据顾客Id判断顾客是否存在
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <returns></returns>
        public async Task<bool> IsCustomerExist(long customerId)
        {
            return await Task.FromResult(_customers.Any(x => x.Id == customerId));
        }
    }
}
