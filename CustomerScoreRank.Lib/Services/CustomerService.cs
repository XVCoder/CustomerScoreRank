using System.Collections.Concurrent;
using CustomerScoreRank.Lib.DataStructures;
using CustomerScoreRank.Lib.Models;

namespace CustomerScoreRank.Lib.Services
{
    public class CustomerService : ICustomerService
    {
        private static readonly SkipList<Customer> _sortedCustomers = new(new CustomerSearchComparer());
        private static readonly ConcurrentDictionary<long, Customer> _dicCustomers = new();
        private static ReaderWriterLockSlim _customersReadWriteLock = new();

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
            Customer updatedCustomer = new();

            try
            {
                if (_customersReadWriteLock.TryEnterWriteLock(TimeSpan.FromSeconds(30)))
                {
                    if (_dicCustomers.TryGetValue(customerId, out Customer? oldCustomer) && _sortedCustomers.Exist(oldCustomer))
                        _sortedCustomers.Remove(oldCustomer);

                    updatedCustomer = _dicCustomers.AddOrUpdate(customerId, new Customer { Id = customerId, Score = score }
                    , (oldKey, oldValue) => new Customer { Id = oldKey, Score = oldValue.Score + score });

                    _sortedCustomers.Add(updatedCustomer);
                }
                else
                {
                    throw new TimeoutException("Update score timeout, please wait a moment then retry!");
                }
            }
            finally
            {
                _customersReadWriteLock.ExitWriteLock();
            }

            return updatedCustomer.Score;
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

            List<Customer> findCustomers = new();
            try
            {
                if (_customersReadWriteLock.TryEnterReadLock(TimeSpan.FromSeconds(30)))
                {
                    findCustomers.AddRange(_sortedCustomers.GetRangeByIndex(start - 1, end - 1));
                }
                else
                {
                    throw new TimeoutException("Get customers timeout, please wait a moment then retry!");
                }
            }
            finally
            {
                _customersReadWriteLock.ExitReadLock();
            }

            List<CustomerScoreRankInfo> result = new();
            for (int i = 0; i < findCustomers.Count; i++)
            {
                result.Add(new CustomerScoreRankInfo
                {
                    CustomerId = findCustomers[i].Id,
                    Score = findCustomers[i].Score,
                    Rank = start + i
                });
            }
            return result;
        }

        /// <summary>
        /// 根据顾客Id获取顾客所处排名前[higherCount]、后[lowerCount]个顾客的数据，包括当前顾客本身
        /// </summary>
        /// <param name="customerId">顾客Id</param>
        /// <param name="higherCount">排在目标顾客前面的顾客个数</param>
        /// <param name="lowerCount">排在目标顾客后面的顾客个数</param>
        /// <returns></returns>
        public List<CustomerScoreRankInfo> GetCustomersByCustomerId(long customerId, int higherCount = 0, int lowerCount = 0)
        {
            if (!_dicCustomers.TryGetValue(customerId, out Customer? targetCustomer))
            {
                return new List<CustomerScoreRankInfo>();
            }

            List<Customer> findCustomers = new();
            try
            {
                if (_customersReadWriteLock.TryEnterReadLock(TimeSpan.FromSeconds(30)))
                {
                    findCustomers = _sortedCustomers.GetRangeAroundItem(targetCustomer, higherCount, lowerCount).ToList();
                }
                else
                {
                    throw new TimeoutException("Get customers timeout, please wait a moment then retry!");
                }
            }
            finally
            {
                _customersReadWriteLock.ExitReadLock();
            }

            List<CustomerScoreRankInfo> result = new();
            for (int i = 0; i < findCustomers.Count; i++)
            {
                result.Add(new CustomerScoreRankInfo
                {
                    CustomerId = findCustomers[i].Id,
                    Score = findCustomers[i].Score,
                    Rank = higherCount + i
                });
            }
            return result;
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

        public long GetCustomerCount() => _dicCustomers.LongCount();
    }
}
