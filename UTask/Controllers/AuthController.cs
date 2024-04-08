using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
namespace HWUTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UTaskService UTaskService;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AuthController(UTaskService authService, RoleManager<IdentityRole> roleManager)
        {
            UTaskService = authService;
            _roleManager = roleManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterationDto registerDto)
        {
            var result = await UTaskService.RegisterUserAsync(registerDto);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginModel)
        {
            if (ModelState.IsValid)
            {
                var token = await UTaskService.LoginUserAsync(loginModel); 
                if (token == null)
                {
                    return Unauthorized();
                }
                //return token as json
                return Ok(token);
            }
            return BadRequest();
        }
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.ForgotPassword(forgotPasswordDto);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.ResetPassword(resetPasswordDto);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string URL)
        {
            await UTaskService.Logout(URL);
            return Ok();
        }
        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromHeader(Name = "Authorization")] string token)
        {
            var user = await UTaskService.GetUser(token);
            if (user != null)
            {
                return Ok(user);
            }
            return BadRequest();
        }
        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser(ProfileDto updateUserDto, [FromHeader(Name = "Authorization")] string token)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.UpdateUser(updateUserDto, token);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest();
            }
            return BadRequest();
        }
        [HttpDelete("deleteAccount")]
        public async Task<IActionResult> DeleteAccount(DeleteUserDto deleteUserDto, [FromHeader(Name = "Authorization")] string token)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.DeleteAccount(deleteUserDto, token);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }
        //[Authorize(Roles = "Admin")]
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser(AppUserDto appUserDto, [FromHeader(Name = "Authorization")] string token)
        {
            var result = await UTaskService.DeleteUser(appUserDto, token);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers([FromHeader(Name = "Authorization")] string token)
        {
            var users = await UTaskService.GetAllUsers(token);
            if (users != null)
            {
                return Ok(users);
            }
            return BadRequest();
        }

        [HttpPut("profilePicture")]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile file, [FromHeader(Name = "Authorization")] string token)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.UpdateProfilePicture(file, token);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest();
            }
            return BadRequest();
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var result = await UTaskService.HealthCheck();
            if (result == 200)
            {
                return Ok();
            }
            return BadRequest();
            
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteUser([FromHeader(Name = "Authorization")] string token, string email)
        {
            if (ModelState.IsValid)
            {
                var result = await UTaskService.InviteFriend(token, email);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }

	}
}
