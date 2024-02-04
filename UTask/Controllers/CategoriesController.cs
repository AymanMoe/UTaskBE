using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using HWUTask.Data;
using UTask.Data.Dtos;
using UTask.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromHeader(Name = "Authorization")] string token, CategoryDtos categoryDto)
        {
            var result = await UTaskService.AddCategoryAsync(token, categoryDto);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromHeader(Name = "Authorization")] string token, int id, CategoryDtos categoryDto)
        {
            var result = await UTaskService.UpdateCategoryAsync(token, id, categoryDto);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [Authorize(Roles = "Admin")]
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
        [Route("AddCategoryToProvider")]
        public async Task<IActionResult> AddCategoryToProvider([FromHeader(Name = "Authorization")] string token, List<int> Categories)
        {
            var result = await UTaskService.AddCategoryToProviderAsync(token, Categories);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }


        [HttpDelete]
        [Route("RemoveCategoryFromProvider")]
        public async Task<IActionResult> RemoveCategoryFromProvider([FromHeader(Name = "Authorization")] string token, List<int> categories)
        {
            var result = await UTaskService.RemoveCategoryFromProviderAsync(token, categories);
            if (result)
            {
                return Ok();
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







    }
}
