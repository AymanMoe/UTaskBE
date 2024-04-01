using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UTask.Data.Dtos;
using UTask.Data.Services;

namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskService UTaskService;

        public ReviewsController(RoleManager<IdentityRole> roleManager, UTaskService uTaskService)
        {
            _roleManager = roleManager;
            UTaskService = uTaskService;
        }

        //Get all reviews for a specific provider
        [HttpGet("{providerId}")]
        public async Task<IActionResult> GetReviews(int providerId)
        {
            var reviews = await UTaskService.GetReviews(providerId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromHeader(Name ="Authorization")] string token, ReviewDto reviewDto)
        {
            var review = await UTaskService.AddReview(token, reviewDto);
            return Ok(review);
        }

        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview([FromHeader(Name = "Authorization")] string token, int reviewId)
        {
           var result = await UTaskService.DeleteReview(token, reviewId);
            if(result)
                return Ok();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview([FromHeader(Name = "Authorization")] string token, int reviewId, ReviewDto reviewDto)
        {
            var review = await UTaskService.UpdateReview(token, reviewId, reviewDto);
            if(review != null)
                return Ok(review);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        
    }
}
