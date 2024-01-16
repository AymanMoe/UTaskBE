using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
using Microsoft.AspNetCore.Identity;
namespace HWUTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AuthController(AuthService authService, RoleManager<IdentityRole> roleManager)
        {
            _authService = authService;
            _roleManager = roleManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterationDto registerDto)
        {
            var result = await _authService.RegisterUserAsync(registerDto);
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
                var token = await _authService.LoginUserAsync(loginModel); 
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
                var result = await _authService.ForgotPassword(forgotPasswordDto);
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
                var result = await _authService.ResetPassword(resetPasswordDto);
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
            await _authService.Logout(URL);
            return Ok();
        }


        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromHeader(Name = "Authorization")] string token)
        {
            var user = await _authService.GetUser(token);
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
                var result = await _authService.UpdateUser(updateUserDto, token);
                if (result)
                {
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }

		[HttpPost("createRole")]

		public async Task<bool> CreateRole(string role)
		{

			var result = await _roleManager.CreateAsync(new IdentityRole(role));
			return result.Succeeded;
		}


	}
}
