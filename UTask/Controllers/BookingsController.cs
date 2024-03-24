using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UTask.Data.Dtos;
using UTask.Data.Services;
using UTask.Models;

namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskService UTaskService;

        public BookingsController(RoleManager<IdentityRole> roleManager, UTaskService uTaskService)
        {
            _roleManager = roleManager;
            UTaskService = uTaskService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromHeader(Name = "Authorization")] string token, BookingDto bookingDto)
        {
            var result = await UTaskService.CreateBookingAsync(token, bookingDto);
            if (result is not null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingDetail([FromHeader(Name = "Authorization")] string token, int bookingId)
        {
            var result = await UTaskService.GetBookingDetailsAsync(token, bookingId);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking([FromHeader(Name = "Authorization")] string token, int bookingId, bool? isConfirmed, BookingDto bookingDto)
        {
            var result = await UTaskService.UpdateBookingAsync(token, bookingId, isConfirmed, bookingDto);
            if (result != 400)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBookings([FromHeader(Name = "Authorization")] string token)
        {
            var result = await UTaskService.GetUserBookingsAsync(token);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> CancelBooking([FromHeader(Name = "Authorization")] string token, int bookingId)
        {
            var result = await UTaskService.CancelBookingAsync(token, bookingId);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("{bookingId}/complete")]
        public async Task<IActionResult> CompleteBooking([FromHeader(Name = "Authorization")] string token, int bookingId)
        {
            var result = await UTaskService.CompleteBookingAsync(token, bookingId);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }


    }
}
