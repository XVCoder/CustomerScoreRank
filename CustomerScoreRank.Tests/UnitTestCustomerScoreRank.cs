using CustomerScoreRank.API.Controllers;
using CustomerScoreRank.Lib.Services;
using System.Diagnostics;
using Xunit.Abstractions;

namespace CustomerScoreRank.Tests
{
    public class UnitTestCustomerScoreRank
    {
        private static ITestOutputHelper _logger;
        private static ICustomerService _customerService;
        private static CustomerController _customerController;
        private static LeaderBoardController _leaderBoardController;

        private const int CUSTOMER_COUNT = 1000;
        private const int REQUEST_COUNT = 10000;
        private const int PAGE_SIZE = 100;

        public UnitTestCustomerScoreRank(ITestOutputHelper logger)
        {
            _logger = logger;
            _customerService = new CustomerService();
            _customerController = new CustomerController(_customerService);
            _leaderBoardController = new LeaderBoardController(_customerService);
        }

        [Fact]
        public void TestUpdateScore()
        {
            _logger.WriteLine($"customer count: {CUSTOMER_COUNT}");
            _logger.WriteLine($"simulaneous request count: {REQUEST_COUNT}");
            Random random = new();
            Stopwatch sw = new();
            sw.Start();
            Parallel.For(1, REQUEST_COUNT, i =>
            {
                _customerController.UpdateScore(random.Next(1, CUSTOMER_COUNT), random.Next(-1000, 1000));
            });
            sw.Stop();
            _logger.WriteLine($"time cost: {sw.Elapsed.TotalMilliseconds} ms");
        }

        [Fact]
        public void TestGetCustomersByRank()
        {
            TestUpdateScore();

            _logger.WriteLine($"\nsimulaneous request count: {REQUEST_COUNT}");
            Random random = new();
            Stopwatch sw = new();
            sw.Start();
            Parallel.For(1, REQUEST_COUNT, i =>
            {
                var r = random.Next(1, CUSTOMER_COUNT);
                _leaderBoardController.GetCustomersByRank(r, r + PAGE_SIZE - 1);
            });
            sw.Stop();
            _logger.WriteLine($"time cost: {sw.Elapsed.TotalMilliseconds} ms");
        }

        [Fact]
        public void GetCustomersByCustomerId()
        {
            TestUpdateScore();

            _logger.WriteLine($"\nsimulaneous request count: {REQUEST_COUNT}");
            Random random = new();
            Stopwatch sw = new();
            sw.Start();
            Parallel.For(1, REQUEST_COUNT, i =>
            {
                _leaderBoardController.GetCustomersByCustomerId(random.Next(1, CUSTOMER_COUNT), random.Next(0, 10), random.Next(0, 10));
            });
            sw.Stop();
            _logger.WriteLine($"time cost: {sw.Elapsed.TotalMilliseconds} ms");
        }
    }
}