using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
namespace HWUTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
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





        

    }
}
