using CustomerScoreRank.Lib.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CustomerScoreRank.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaderBoardController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public LeaderBoardController(
            ICustomerService customerService
            )
        {
            _customerService = customerService;
        }

        [HttpGet(Name = nameof(GetCustomersByRank))]
        public IActionResult GetCustomersByRank(
            [Required]
            [Range(1, int.MaxValue)]
            int start,
            [Required]
            [Range(1, int.MaxValue)]
            int end)
        {
            if (start > end)
            {
                return BadRequest($"start({start}) should be less then end({end})!");
            }

            if (_customerService.IsCustomerListEmpty())
            {
                return new EmptyResult();
            }

            return Ok(_customerService.GetCustomersByRank(start, end));
        }

        [HttpGet("{customerId}", Name = nameof(GetCustomersByCustomerId))]
        public IActionResult GetCustomersByCustomerId(
            [Required]
            long customerId,
            [Required]
            [Range(0, int.MaxValue)]
            int high,
            [Required]
            [Range(0, int.MaxValue)]
            int low)
        {
            if (!_customerService.IsCustomerExist(customerId))
            {
                return NotFound($"customer id: {customerId}");
            }

            return Ok(_customerService.GetCustomersByCustomerId(customerId, high, low));
        }
    }
}