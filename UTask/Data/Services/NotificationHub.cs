using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Composition.Convention;
using System.Threading.Tasks;
using UTask.Data.Contexts;
using UTask.Data.Dtos;
using UTask.Models;

namespace UTask.Data.Services
{
    public class NotificationHub : Hub
    {
        private readonly UTaskDbContext _dbContext;

        public NotificationHub(UTaskDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.User.Identity.Name;
            Console.WriteLine("Connected", userId);
            var context = Context;
            await base.OnConnectedAsync();
            await _dbContext.SaveChangesAsync();
            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connection = await _dbContext.ConnectionMappings.FindAsync(Context.ConnectionId);
            if (connection != null)
            {
                _dbContext.ConnectionMappings.Remove(connection);
                await _dbContext.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ReceiveNotifications(NotificationDto notification)
        {
            Console.WriteLine("Recieve message to: ", notification);
                await Clients.All.SendAsync("ReceiveNotifications", notification);
            
        }

        public async Task Login(string AppUserId)
        {
            Console.WriteLine("Logging in: ", AppUserId);
            var connection = await _dbContext.ConnectionMappings.FirstOrDefaultAsync(c => c.UserId == AppUserId);
           //Refactor, too many database queries 
            if (connection == null)
            {
                _dbContext.ConnectionMappings.Add(new ConnectionMapping { UserId = AppUserId, ConnectionId = Context.ConnectionId });
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _dbContext.ConnectionMappings.RemoveRange(connection);
                await _dbContext.SaveChangesAsync();
                connection.UserId = AppUserId;
                connection.ConnectionId = Context.ConnectionId;
                _dbContext.ConnectionMappings.Add(connection);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Logout(string AppUserId)
        {
            Console.WriteLine("Logging out: ", AppUserId);
            var connection = await _dbContext.ConnectionMappings.FirstOrDefaultAsync(c => c.UserId == AppUserId);
            if (connection != null)
            {
                _dbContext.ConnectionMappings.RemoveRange(connection);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task NewBooking(string message)
        {
            Console.WriteLine("NewBooking: ", message);
            await Clients.All.SendAsync("SendMessage", message);
        }
    }

}
