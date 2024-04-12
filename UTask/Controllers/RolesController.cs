using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
using Microsoft.AspNetCore.Identity;
using UTask.Data.Contexts;

namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        //Add RoleManager
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UTaskService UTaskService;
        private readonly UTaskDbContext _context;

        public RolesController(RoleManager<IdentityRole> roleManager, UTaskService uTaskService, UserManager<IdentityUser> userManager, UTaskDbContext uTaskDbContext)
        {
            _roleManager = roleManager;
            UTaskService = uTaskService;
            _userManager = userManager;
        }
        //Uncomment to add new admins
        /*
        [HttpPost]
        public async Task<IActionResult> CreateRole(string role)
        {
            //Use RoleManager to create a new role
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest();
        }

        
        [HttpGet("{email}")]
        public async Task<IActionResult> GetRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        
        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded)
            {
                var result2 = await _userManager.AddToRoleAsync(user, role);
                if (result2.Succeeded)
                {
                    return Ok();
                }
            }
            return BadRequest();
        }*/
    }
}
