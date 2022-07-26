using CustomerScoreRank.Models;
using CustomerScoreRank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CustomerScoreRank.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeadBoardController : ControllerBase
    {
        private readonly ILogger<LeadBoardController> _logger;
        private readonly ICustomerService _customerService;

        public LeadBoardController(
            ILogger<LeadBoardController> logger,
            ICustomerService customerService
            )
        {
            _logger = logger;
            _customerService = customerService;
        }

        [HttpGet(Name = nameof(GetCustomersByRank))]
        public async Task<IActionResult> GetCustomersByRank(
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

            return Ok(await _customerService.GetCustomersByRank(start, end));
        }

        [HttpGet("{customerId}", Name = nameof(GetCustomersByCustomerId))]
        public async Task<IActionResult> GetCustomersByCustomerId(
            [Required]
            long customerId,
            [Required]
            [Range(0, int.MaxValue)]
            int high,
            [Required]
            [Range(0, int.MaxValue)]
            int low)
        {
            if (!await _customerService.IsCustomerExist(customerId))
            {
                return NotFound($"customer id: {customerId}");
            }

            return Ok(await _customerService.GetCustomersByCustomerId(customerId, high, low));
        }
    }
}