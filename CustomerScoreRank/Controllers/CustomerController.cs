using CustomerScoreRank.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CustomerScoreRank.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;

        public CustomerController(
            ILogger<CustomerController> logger,
            ICustomerService customerService
            )
        {
            _logger = logger;
            _customerService = customerService;
        }

        [HttpPost("{customerId}/score/{score}", Name = nameof(UpdateScore))]
        public async Task<IActionResult> UpdateScore(
            long customerId,
            [Range(-1000, 1000)]
            decimal score
            )
        {
            return Ok(await _customerService.UpdateScore(customerId, score));
        }
    }
}