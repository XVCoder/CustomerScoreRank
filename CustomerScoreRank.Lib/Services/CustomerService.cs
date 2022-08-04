using CustomerScoreRank.Lib.Models;
using System.Collections;
using System.Collections.Concurrent;

namespace CustomerScoreRank.Lib.Services
{
    public class CustomerService : ICustomerService
    {
        private static List<Customer> _sortedCustomers = new();
        private static readonly ConcurrentDictionary<long, Customer> _dicCustomers = new();

        public CustomerService()
        {
        }

        /// <summary>
        /// 更新顾客的分数（累加）
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <param name="score">分数</param>
        /// <returns></returns>
        public decimal UpdateScore(long customerId, decimal score)
        {
            lock (_dicCustomers)
            {
                lock ((_sortedCustomers as ICollection).SyncRoot)
                {
                    Customer customer;
                    if (_dicCustomers.ContainsKey(customerId))
                    {// Update
                        customer = _dicCustomers[customerId];
                        lock (customer)
                        {
                            customer.Score += score;
                            _sortedCustomers.Sort();
                            return customer.Score;
                        }
                    }
                    else
                    {// Add
                        customer = new Customer { Id = customerId, Score = score };
                        _dicCustomers[customerId] = customer;
                        _sortedCustomers.Add(customer);
                        _sortedCustomers.Sort();
                        return score;
                    }
                }
            }
        }

        /// <summary>
        /// 根据排名获得顾客列表
        /// </summary>
        /// <param name="start">起始排名</param>
        /// <param name="end">结束排名</param>
        /// <returns></returns>
        public List<CustomerScoreRankInfo> GetCustomersByRank(int start, int end)
        {
            if (start > _sortedCustomers.Count)
            {
                return new List<CustomerScoreRankInfo>();
            }

            var findCustomers = _sortedCustomers.Skip(start - 1).Take(end - start + 1).ToList();
            var customerScoreRankInfos = new List<CustomerScoreRankInfo>();
            for (int i = 0; i < findCustomers.Count; i++)
            {
                customerScoreRankInfos.Add(new CustomerScoreRankInfo
                {
                    CustomerId = findCustomers[i].Id,
                    Score = findCustomers[i].Score,
                    Rank = start + i
                });
            }
            return customerScoreRankInfos;
        }

        /// <summary>
        /// 根据顾客Id获取顾客所处排名前[high]、后[low]个顾客的数据
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <param name="high">排在目标顾客前面的顾客个数</param>
        /// <param name="low">排在目标顾客后面的顾客个数</param>
        /// <returns></returns>
        public List<CustomerScoreRankInfo> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0)
        {
            if (!_dicCustomers.ContainsKey(customerId))
            {
                return new List<CustomerScoreRankInfo>();
            }

            // 获取目标顾客在分数排名中的位置（列表索引）
            var indexOfTargetCustomer = ~_sortedCustomers.BinarySearch(_dicCustomers[customerId]);

            // 防止索引溢出
            high = indexOfTargetCustomer - high < 0 ? 0 : high;
            low = indexOfTargetCustomer + low > _sortedCustomers.Count - 1 ? _sortedCustomers.Count - 1 : low;

            return GetCustomersByRank(indexOfTargetCustomer - high + 1, indexOfTargetCustomer + low + 1);
        }

        /// <summary>
        /// 根据顾客Id判断顾客是否存在
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <returns></returns>
        public bool IsCustomerExist(long customerId)
        {
            return _dicCustomers.Any(x => x.Key == customerId);
        }

        /// <summary>
        /// 判断顾客列表是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsCustomerListEmpty()
        {
            return !_dicCustomers.Any();
        }
    }
}
