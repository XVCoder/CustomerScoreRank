using System.ComponentModel.DataAnnotations;
using CustomerScoreRank.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerScoreRank.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public TestController(
            ICustomerService customerService
            )
        {
            _customerService = customerService;
        }

        [HttpPost("GenerateCustomers/{executeUpdateActionsCount}", Name = nameof(GenerateCustomers))]
        public IActionResult GenerateCustomers(
            [Range(0, int.MaxValue)]
            int executeUpdateActionsCount
            )
        {
            Random random = new();
            Parallel.For(1, executeUpdateActionsCount, i =>
            {
                _customerService.UpdateScore(random.Next(1, executeUpdateActionsCount), random.Next(-1000, 1000));
            });
            return Ok($"Current customers count: {_customerService.GetCustomerCount()}");
        }
    }
}