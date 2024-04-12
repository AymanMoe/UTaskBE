using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UTask.Data.Services;
using UTask.Data.Dtos;
using Microsoft.AspNetCore.SignalR;
using UTask.Models;
using System.Security.Claims;
using UTask.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Linq;

namespace UTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //The Notification Controller corresponds to the Real-time notification feature requirement. 
    public class NotificationsController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskService UTaskService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly UTaskDbContext _dbContext;
        public NotificationsController(RoleManager<IdentityRole> roleManager, UTaskService uTaskService, IHubContext<NotificationHub> hubContext, UTaskDbContext uTaskDbContext)
        {
            _roleManager = roleManager;
            UTaskService = uTaskService;
            _notificationHub = hubContext;
            _dbContext = uTaskDbContext;
        }
        [HttpGet]
        public async Task<List<Notification>> GetNotifications([FromHeader(Name = "Authorization")] string token)
        {
            List<Notification> notifications = new List<Notification>();
            (string userId, int id, string role) = UTaskService.DecodeToken(token);
            if (role == "Client") 
            {
                var clientNotifications = await _dbContext.ClientNotifications.Where(c => c.ClientId == id).ToListAsync();

                if (clientNotifications != null)
                {
                   foreach(var cn in clientNotifications)
                    {
                        var ntf = _dbContext.Notifications.FirstOrDefault(n => n.Id == cn.NotificationId);
                        notifications.Add(ntf);
                    }
                    return notifications;
                }
                else { throw new Exception("Client notification not found");}
            } else if (role == "Provider"){
                var providerNotifications = await _dbContext.ProviderNotifications.Where(p => p.ProviderId == id).ToListAsync();
                if (providerNotifications != null)
                {

                    foreach(var pn in providerNotifications)
                    {
                        var ntf = _dbContext.Notifications.FirstOrDefault(n => n.Id == pn.NotificationId);
                        notifications.Add(ntf);
                    }
                    return notifications;
                }
                else { throw new Exception("Provider notification not found");}

            }
            return null;
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveNotification([FromHeader(Name = "Authorization")] string token, int id)
        {
            var result = await UTaskService.RemoveNotification(token, id);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
