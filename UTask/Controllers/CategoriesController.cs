using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UTask.Models;
namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //CategoriesController corresponds to the Service Provider Categories feature requirement.
    public class CategoriesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskService UTaskService;

        public CategoriesController(RoleManager<IdentityRole> roleManager, UTaskService uTaskService)
        {
            _roleManager = roleManager;
            UTaskService = uTaskService;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await UTaskService.GetCategoriesAsync();
            return Ok(categories);
        }
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromHeader(Name = "Authorization")] string token, Category category)
        {
            var response = await UTaskService.AddCategoryAsync(token, category);
            if (response!=null)
            {
                return Ok(response);
            }
            return BadRequest();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromHeader(Name = "Authorization")] string token, int id, Category category)
        {
            var result = await UTaskService.UpdateCategoryAsync(token, id, category);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromHeader(Name = "Authorization")] string token, int id)
        {
            var result = await UTaskService.DeleteCategoryAsync(token, id);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("setSubscriptions")]
        public async Task<IActionResult> SubscribeToCategory([FromHeader(Name = "Authorization")] string token, SubscriptionDto subscriptionDto)
        {
            var result = await UTaskService.SetCategorySubscriptions(token, subscriptionDto);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
        [HttpGet]
        [Route("getSubscriptions")]
        public async Task<IActionResult> GetSubscriptions([FromHeader(Name = "Authorization")] string token)
        {
            var result = await UTaskService.GetCategorySubscriptions(token);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }
        [HttpGet]
        [Route("GetProvidersByCategoryId")]
        public async Task<IActionResult> GetProvidersByCategoryId(int id)
        {
            var providers = await UTaskService.GetProvidersByCategoryIdAsync(id);
            return Ok(providers);
        }

        [HttpGet]
        [Route("GetProviderById")]
        public async Task<IActionResult> GetProviderById([FromHeader(Name = "Authorization")] string token,int id)
        {
            var provider = await UTaskService.GetProviderByIdAsync(id);
            return Ok(provider);
        }

    }
}
