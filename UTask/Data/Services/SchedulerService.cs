using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Owin.BuilderProperties;
using UTask.Data.Contexts;
using UTask.Data.Dtos;
using UTask.Models;
namespace UTask.Data.Services
{
    public class SchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly UTaskDbContext _dbContext;
        private readonly UTaskService _uTaskService;
        public SchedulerService(IBackgroundJobClient backgroundJobClient, UTaskDbContext uTaskDbContext, UTaskService uTaskService)
        {
            _backgroundJobClient = backgroundJobClient;
            _dbContext = uTaskDbContext;
            _uTaskService = uTaskService;
        }

        public void SendReminders ()
        {
            //get all bookings that are confirmed and have a service date
            var bookings = _dbContext.Bookings.Where(b => b.Status == "Confirmed" && b.ServiceDate != null); //TODO: the date is in the future
            foreach (var booking in bookings)
            {
                ScheduleBookingReminder(booking, booking.ServiceDate.Value.AddHours(-24));
            }
            //TODO: get all bookings that are confirmed and have a service date that is today
            var todayBookings = _dbContext.Bookings.Where(b => b.Status == "Confirmed" && b.ServiceDate != null && b.ServiceDate.Value.Date == DateTime.Today);
            foreach (var booking in todayBookings)
            {
                //TODO
            }
        }
        public void ScheduleBookingReminder(Booking booking, DateTime serviceDate)
        {
            _backgroundJobClient.Schedule(() => SendBookingReminder(booking), serviceDate);
        }
        public async void SendBookingReminder(Booking booking)
        {
            var serviceName = booking.Category.ServiceName;
            var Address = booking.Address;
            var client = booking.Client;
            var provider = booking.Provider;
            //Create a reminder notification to client and provider
            await _uTaskService.CreateNotification("Provider", 
                new NotificationDto { 
                    Body = $"You have a booking appointment tomorrow with {client.FirstName} for {serviceName} service in {Address.StreetAddress} on {booking.ServiceDate}",
                    ProviderId = booking.ProviderId,
                    Type = "Reminder",
                    Title = "Booking Reminder",
                    CreatedAt = DateTime.Now,
                    Data = new { ClientId = booking.ClientId, AddressId = booking.Address.AddressId, CategoryId = booking.Category.ServiceName, BookingId = booking.Id },


                });

            await _uTaskService.CreateNotification("Client",
                new NotificationDto
                {
                    Body = $"You have a booking appointment tomorrow with {provider.FirstName} for {serviceName} service in {Address.StreetAddress} on {booking.ServiceDate}",
                    ClientId = booking.ClientId,
                    Type = "Reminder",
                    Title = "Booking Reminder",
                    CreatedAt = DateTime.Now,
                    Data = new { ClientId = booking.ClientId, AddressId = booking.Address.AddressId, CategoryId = booking.Category.ServiceName, BookingId = booking.Id },


                }
                );
            
        }
    }
}
