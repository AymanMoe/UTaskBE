using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UTask.Data.Services;
using UTask.Data.Contexts;
using UTask.Data.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
namespace UTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers();
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<UTaskDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.Configure<SenderOptionsDto>(builder.Configuration);

            

            builder.Services.AddScoped<AuthService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    });
            });

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
                ); ;
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UTask API", Version = "v1" });
            });
            var app = builder.Build();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UTask API V1");
                    c.RoutePrefix = string.Empty; // Set the root path for Swagger UI
                });
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("AllowVueApp");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            

            

            app.Run();
            
        }
    }
}