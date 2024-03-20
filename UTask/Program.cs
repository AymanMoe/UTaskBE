using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UTask.Data.Services;
using UTask.Data.Contexts;
using UTask.Data.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Connections;
using System.Net.WebSockets;
using Hangfire.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using System.Configuration;
using System.Text.Json.Serialization;
namespace UTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = null;
            });
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
           
            builder.Services.AddDbContext<UTaskDbContext>(options => options.UseSqlServer(connection));
            
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.Configure<SenderOptionsDto>(builder.Configuration);
            

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<UTaskDbContext>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateActor = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration.GetSection("Jwt:issuer").Value,
                    ValidAudience = builder.Configuration.GetSection("Jwt:audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:key").Value))

                }
                );

            builder.Services.AddScoped<UTaskService>();
            builder.Services.AddScoped<NotificationHub>();

            HubOptions options = new HubOptions();
            builder.Services.AddSignalR().AddHubOptions<NotificationHub>(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.AddCors(options =>
            {

                options.AddPolicy("AllowVueApp",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
        });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UTask API", Version = "v1" });
            });
            
            builder.Services.AddHangfire(configuration => configuration
             .UseSqlServerStorage(connection, new SqlServerStorageOptions
             {
                 CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                 SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                 QueuePollInterval = TimeSpan.Zero,
                 UseRecommendedIsolationLevel = true,
                 DisableGlobalLocks = true
             })
             .WithJobExpirationTimeout(TimeSpan.FromDays(30))
             .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
             .UseSimpleAssemblyNameTypeSerializer()
             .UseRecommendedSerializerSettings());
            builder.Services.BuildServiceProvider().GetRequiredService<IGlobalConfiguration>();
            builder.Services.AddScoped<SchedulerService>();
            RecurringJob.AddOrUpdate<SchedulerService>(x => x.SendReminders(), Cron.Daily);
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UTask API V1");
                c.RoutePrefix = string.Empty; // Set the root path for Swagger UI
            });
            app.UseCors("AllowVueApp");
            app.UseHangfireDashboard();

/*            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/NotificationHub")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    }
                }
                else
                {
                    await next(context);
                }

            });*/

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            var webSocketOptions = new Microsoft.AspNetCore.Builder.WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };
            app.MapHub<NotificationHub>("/NotificationHub", options => {
                options.Transports =
        HttpTransportType.WebSockets |
        HttpTransportType.LongPolling;
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.Run();
            
        }
      /*  private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }*/
    }
}