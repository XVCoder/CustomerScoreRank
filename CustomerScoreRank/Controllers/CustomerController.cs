using System.ComponentModel.DataAnnotations;
using CustomerScoreRank.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerScoreRank.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(
            ICustomerService customerService
            )
        {
            _customerService = customerService;
        }

        [HttpPost("{customerId}/score/{score}", Name = nameof(UpdateScore))]
        public IActionResult UpdateScore(
            long customerId,
            [Range(-1000, 1000)]
            decimal score
            )
        {
            return Ok(_customerService.UpdateScore(customerId, score));
        }
    }
}