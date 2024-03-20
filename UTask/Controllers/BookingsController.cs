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

        /**
         * BookingController:
            Create Booking: POST /api/bookings
            Get Booking Details: GET /api/bookings/{bookingId}
            Get User's Bookings: GET /api/users/{userId}/bookings
            Cancel Booking: DELETE /api/bookings/{bookingId}
            Update Booking: PUT /api/bookings/{bookingId}
            Get Provider's Bookings: GET /api/providers/{providerId}/bookings
         */
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

        //Http put UpdateBooking
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
        


        /***CollectResponses: This step involves collecting responses from the notified providers.
         * The providers can either accept or reject the booking request. if they accept, the booking is confirmed with the selected provider.*/
/*
        [HttpPut("{bookingId}")]
        public async Task<IActionResult> CollectResponse([FromHeader(Name = "Authorization")] string token, int bookingId, bool isConfirmed)
        {
            (string userId, int id, string role) = UTaskService.DecodeToken(token);
            if (role == "Provider")
            {
                var booking = await UTaskService.GetBookingDetailsAsync(token, bookingId);
                var result = await UTaskService.ConfirmBooking( isConfirmed, booking, id);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            } else
            {
                return Unauthorized();
            }
        }*/


    }
}
