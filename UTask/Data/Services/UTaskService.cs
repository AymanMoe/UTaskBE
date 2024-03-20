using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using UTask.Data.Contexts;
using UTask.Models;
using UTask.Data.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using HWUTask.Data;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Humanizer.Localisation.TimeToClockNotation;
using System.Net;
using NuGet.Protocol;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Configuration.Provider;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using Microsoft.Owin.BuilderProperties;
using Address = UTask.Models.Address;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace UTask.Data.Services
{
    public class UTaskService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> _notificationHub;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UTaskService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
           SignInManager<IdentityUser> signInManager
           , IConfiguration config, IEmailSender emailSender, UTaskDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub> notificationHub)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _config = config;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _notificationHub = notificationHub;
        }
        public (string, int, string) DecodeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return (null, -1, null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config.GetSection("Jwt:issuer").Value,
                ValidAudience = _config.GetSection("Jwt:audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Jwt:key").Value))
            };

            try
            {
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token.Replace("Bearer ", ""), tokenValidationParameters, out SecurityToken validatedToken);
                var userId = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                var id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
                return (userId, Int32.Parse(id), role);

            }
            catch (Exception e)
            {
                return (null, -1, null);
            }
        }

        public async Task<bool> CreateNotification(string role, NotificationDto ntf)
        {
            try
            {
                var notification = new Notification
                {
                    Action = ntf.Action,
                    Body = ntf.Body,
                    Title = ntf.Title,
                    CreatedAt = DateTime.Now,
                    Type = ntf.Type,
                    Data = JsonConvert.SerializeObject(ntf.Data),
                    IsRead = false
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                if (role == "Client")
                {
                    var client = await _context.Clients.FindAsync(ntf.ClientId);
                    if (client == null)
                    {
                        return false;
                    }

                    ntf.Id = notification.Id;
                    var ConnectionIds = await _context.ConnectionMappings.Where(c => c.UserId == client.AppUserName).Select(x => x.ConnectionId).ToListAsync();

                    if (ConnectionIds.Count > 0) { await _notificationHub.Clients.Client(ConnectionIds[0]).SendAsync("ReceiveNotifications", ntf); }

                    var dbClientNotification = await _context.ClientNotifications.FirstOrDefaultAsync(cn => cn.ClientId == client.Id && cn.NotificationId == notification.Id);
                    if (dbClientNotification != null)
                    {
                        return true;
                    }

                    var clientNotification = new ClientNotification
                    {
                        ClientId = client.Id,
                        NotificationId = notification.Id
                    };

                    _context.ClientNotifications.Add(clientNotification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else if (role == "Provider")
                {
                    var provider = await _context.Providers.FindAsync(ntf.ProviderId);
                    if (provider == null)
                    {
                        return false;
                    }
                   
                    ntf.Id = notification.Id;
                    var ConnectionIds = await _context.ConnectionMappings.Where(c => c.UserId == provider.AppUserName).Select(x => x.ConnectionId).ToListAsync();

                    if (ConnectionIds.Count > 0)
                    {
                        await _notificationHub.Clients.Client(ConnectionIds[0]).SendAsync("ReceiveNotifications", ntf);

                    }
                    var dbProviderNotification = await _context.ProviderNotifications.FirstOrDefaultAsync(pn => pn.ProviderId == provider.Id && pn.NotificationId == notification.Id);
                    if (dbProviderNotification != null)
                    {
                        return true;
                    }

                    var providerNotification = new ProviderNotification
                    {
                        ProviderId = provider.Id,
                        NotificationId = notification.Id
                    };



                    _context.ProviderNotifications.Add(providerNotification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> RemoveNotification(string token, int NotificationId)
        {
            (string userId, int id, string role) = DecodeToken(token);
            try
            {
                if (role == "Client")
                {
                    var clientNotification = await _context.ClientNotifications.FirstOrDefaultAsync(cn => cn.NotificationId == NotificationId);
                    var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == NotificationId);
                    if (notification == null || clientNotification == null)
                    {
                        return false;
                    }

                    _context.ClientNotifications.Remove(clientNotification);
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else if (role == "Provider")
                {
                    var providerNotification = await _context.ProviderNotifications.FirstOrDefaultAsync(pn => pn.NotificationId == NotificationId);
                    var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == NotificationId);
                    if (notification == null || providerNotification == null)
                    {
                        return false;
                    }

                    _context.ProviderNotifications.Remove(providerNotification);
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        //=======================================================================================================
        //                                  Authroization and Authentication
        //=======================================================================================================
        public async Task<IdentityResult> RegisterUserAsync(RegisterationDto rdto)
        {
            if (!await _roleManager.RoleExistsAsync(rdto.Type))
            {
                return null;
            }
            var coords = await getCoords(rdto.Address);
            var lat = coords.Item1;
            var lon = coords.Item2;
            if (lat == -1 || lon == -1) { return null; }
            var appUser = new AppUser
            {
                FirstName = rdto.FirstName,
                LastName = rdto.LastName,
                UserName = rdto.Email,
                Email = rdto.Email,
                Type = rdto.Type == "Client" ? UserType.Client : (rdto.Type == "Provider" ? UserType.Provider : UserType.Admin)
            };
            var result = await _userManager.CreateAsync(appUser, rdto.Password);
            await _userManager.AddToRoleAsync(appUser, rdto.Type);
            if (result.Succeeded)
            {
                //TODO: Rewrite

                _context.Addresses.Add(new Address
                {
                    StreetAddress = rdto.Address.StreetAddress,
                    City = rdto.Address.City,
                    PostalCode = rdto.Address.PostalCode,
                    Country = rdto.Address.Country,
                    Latitude = lat,
                    Longitude = lon,
                    AppUserName = appUser.UserName,
                    AppUser = appUser
                });
                _context.SaveChanges();
                var address = _context.Addresses.FirstOrDefault(a => a.City == rdto.Address.City && a.Country == rdto.Address.Country && a.StreetAddress == rdto.Address.StreetAddress && a.PostalCode == rdto.Address.PostalCode);
                // TODO END // 
                if (rdto.Type == "Client")
                {

                    _context.Clients.Add(new Client
                    {
                        AppUser = appUser,
                        AppUserName = appUser.UserName,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName
                    });

                }
                else if (rdto.Type == "Provider")
                {
                    var provider = _context.Providers.Add(new Provider
                    {
                        AppUser = appUser,
                        AppUserName = appUser.UserName,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName
                    });
                    _context.SaveChanges();
                    foreach (var category in rdto.Categories)
                    {
                        var c = _context.Categories.FirstOrDefault(c => c.Id == category);
                        if (c == null)
                        {
                            continue;
                        }
                        _context.ProviderCategories.Add(new ProviderCategory
                        {
                            ProviderId = provider.Entity.Id,
                            CategoryId = c.Id
                        });
                    }
                }
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(appUser, isPersistent: false);
                return result;
            }
            return result;
        }

        public async Task<object> LoginUserAsync(LoginDto ldto)
        {
            var result = await _signInManager.PasswordSignInAsync(ldto.Email, ldto.Password, ldto.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(ldto.Email);
                var role = await _userManager.GetRolesAsync(user);
                var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AppUserName == user.Id);
                if (role != null)
                {
                    if (role[0] == "Client")
                    {
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.AppUser == user);

                        if (client == null) { return null; }
                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], client.Id),
                            role = role[0],
                            user = new
                            {
                                id = user.Id,
                                firstName = client.FirstName,
                                lastName = client.LastName,
                                email = user.Email,
                                address = new
                                {
                                    streetAddress = address.StreetAddress,
                                    city = address.City,
                                    postalCode = address.PostalCode,
                                    country = address.Country
                                }
                            }
                        };
                        //await _notificationHub.OnConnectedAsync(); using hub context
                        return response;
                    }
                    else
                 if (role[0] == "Admin")
                    {
                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], 0),
                            role = role,
                            user = new { id = user.Id, Email = user.Email }
                        };
                        //await _notificationHub.OnConnectedAsync();
                        return response;
                    }
                    else
                    {
                        var provider = await _context.Providers.FirstOrDefaultAsync(p => p.AppUser == user);
                        if (provider == null) { return null; }
                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], provider.Id),
                            role = role,
                            user = new
                            {
                                id = user.Id,
                                firstName = provider.FirstName,
                                lastName = provider.LastName,
                                email = user.Email,
                                address = new
                                {
                                    streetAddress = address.StreetAddress,
                                    city = address.City,
                                    postalCode = address.PostalCode,
                                    country = address.Country
                                }
                            }
                        };
                        //await _notificationHub.OnConnectedAsync();
                        return response;
                    }
                }
                return null;

            }

            return null;
        }

        private string GenerateTokenString(string userId, string role, int Id)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Id.ToString())
            };

            var SecurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Jwt:key").Value));
            SigningCredentials SigningCred = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                issuer: _config.GetSection("Jwt:issuer").Value,
                audience: _config.GetSection("Jwt:audience").Value,
                signingCredentials: SigningCred
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return tokenString;
        }

        //2 TODO
        public async Task<bool> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);
                //TODO: CHANGE THE URL TO use the client app url
                var url = $"http://localhost:5204/resetpassword?email={forgotPasswordDto.Email}&token={validToken}";
                //TODO: CHANGE THE MESSAGE TO use the client app url
                var message = $"<p>Please reset your password by <a href='{url}'>clicking here</a></p>";
                await _emailSender.SendEmailAsync(forgotPasswordDto.Email, "Reset Password", message);
                return true;
            }
            return false;
        }

        public Task<bool> ResetPassword(ResetPasswordDto resetPasswordDto)
        {

            var decodedToken = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
            var normalToken = Encoding.UTF8.GetString(decodedToken);


            var user = _userManager.FindByEmailAsync(resetPasswordDto.Email).Result;
            var result = _userManager.ResetPasswordAsync(user, normalToken, resetPasswordDto.NewPassword).Result;
            if (result.Succeeded)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);

        }

        public async Task<bool> Logout(string returnUrl)
        {

            await _signInManager.SignOutAsync();
            return true;
        }
        //=======================================================================================================
        //                                  Profile
        //=======================================================================================================
        public async Task<object> GetUser(string token)
        {

            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1) { return null; }
            var user = await _userManager.FindByIdAsync(userId);
            var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AppUserName == user.Id);
            if (user == null) { return null; }
            if (role == "Client")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
                if (client == null) { return null; }
                var response = new
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    role = role,
                    Phone = client.Phone,
                    Email = user.Email,
                    Address = new AddressDto
                    {
                        StreetAddress = address?.StreetAddress,
                        City = address?.City,
                        PostalCode = address?.PostalCode,
                        Province = address?.Province,
                        Country = address?.Country
                    }
                };
                return response;

            }
            else if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                if (provider == null) { return null; }
                var response = new
                {
                    FirstName = provider.FirstName,
                    LastName = provider.LastName,
                    role = role,
                    Phone = provider.Phone,
                    Email = user.Email,
                    Address = new AddressDto
                    {
                        StreetAddress = address?.StreetAddress,
                        City = address?.City,
                        PostalCode = address?.PostalCode,
                        Province = address?.Province,
                        Country = address?.Country
                    }


                };
                return response;
            }
            else if (role == "Admin")
            {
                var response = new
                {
                    Email = user.Email,
                    role = role
                };
                return response;
            }
            else
            {
                throw new Exception("Invalid role");
            }
        }

        public Task<bool> UpdateUser(ProfileDto updateUserDto, string token)
        {

            (string userId, int id, string role) = DecodeToken(token);
            
            if (userId == null || id == -1)
            {
                return Task.FromResult(false);
            }
            
            var user = _userManager.FindByIdAsync(userId).Result;

            var address = _context.Addresses.FirstOrDefault(c => c.AppUserName == user.Id);
            if (address == null) { return Task.FromResult(false); }
            var coords = getCoords(updateUserDto.Address);
            var lat = coords.Result.Item1;
            var lon = coords.Result.Item2;
            if (lat == -1 || lon == -1) { return Task.FromResult(false); }
            if (role == "Client")
            {
                var client = _context.Clients.FirstOrDefault(c => c.Id == id);

                if (client == null) { return Task.FromResult(false); }
                client.FirstName = updateUserDto.FirstName;
                client.LastName = updateUserDto.LastName;
                client.Phone = updateUserDto.Phone;
                user.PhoneNumber = updateUserDto.Phone;
                address.StreetAddress = updateUserDto.Address.StreetAddress;
                address.City = updateUserDto.Address.City;
                address.PostalCode = updateUserDto.Address.PostalCode;
                address.Province = updateUserDto.Address.Province;
                address.Country = updateUserDto.Address.Country;
                address.Latitude = lat;
                address.Longitude = lon;

                _userManager.UpdateAsync(user);
                _context.SaveChangesAsync();
                return Task.FromResult(true);
            }
            else if (role == "Provider")
            {
                var provider = _context.Providers.FirstOrDefault(p => p.Id == id);
                if (provider == null) { return Task.FromResult(false); }
                provider.FirstName = updateUserDto.FirstName;
                provider.LastName = updateUserDto.LastName;
                provider.Phone = updateUserDto.Phone;
                address.StreetAddress = updateUserDto.Address.StreetAddress;
                address.City = updateUserDto.Address.City;
                address.PostalCode = updateUserDto.Address.PostalCode;
                address.Province = updateUserDto.Address.Province;
                address.Country = updateUserDto.Address.Country;
                _userManager.UpdateAsync(user);
                _context.SaveChangesAsync();
                return Task.FromResult(true);
            }
            else if (role == "Admin") { 
                //TODO: Enable admin to update anyone's profile
                return Task.FromResult(true); }
            else
            {
                throw new Exception("Invalid role");
            }
        }

        private async Task<(double, double)> getCoords(AddressDto addressDto)
        {
            var request = new
            {
                query = addressDto.StreetAddress + " " + addressDto.City + " " + addressDto.Country
            };
            var subscriptionKey = "pUBV4lwanhEEiGQZSgR4KH_dqMv5QjunNm8A2YIwS1c";
            using (var client = new HttpClient())
            {
                var baseUrl = "https://atlas.microsoft.com/search/address/json";
                var queryParams = $"?subscription-key={subscriptionKey}&api-version=1.0&countrySet=CA&language=en-CA&query=" + request.query;
                var requestUrl = baseUrl + queryParams;

                var response = await client.GetAsync(requestUrl);
                Console.WriteLine(response.RequestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(content.Result);

                    var latitude = (double)result.results[0].position.lat;
                    var longitude = (double)result.results[0].position.lon;

                    Console.WriteLine($"Latitude: {latitude}, Longitude: {longitude}");
                    return (latitude, longitude);
                }
                else
                {
                    Console.WriteLine($"Error: {response.ReasonPhrase}, {response.RequestMessage}");
                    return (-1, -1);
                }
            }
        }

        public async Task<bool> DeleteAccount(DeleteUserDto deleteUserDto, string token)
        {

            (string userId, int id, string role) = DecodeToken(token);
            var user = await _userManager.FindByIdAsync(userId);
            if (id == -1 || user == null) { return false; }
            var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AppUserName == user.UserName);
            if (role == "Client")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
                if (client == null) { return false; }
                _context.Clients.Remove(client);
                if (address != null)
                {
                    _context.Addresses.Remove(address);
                }
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                return true;
            }
            else if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                if (provider == null) { return false; }
                _context.Providers.Remove(provider);
                if (address != null)
                {
                    _context.Addresses.Remove(address);
                }
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                return true;
            }
            else if (role == "Admin")
            {
                await _userManager.DeleteAsync(user);
                return true;
            }
            else
            {
                throw new Exception("Invalid role");
            }
        }
        //                                          Admin Privileges

        public async Task<bool> DeleteUser(AppUserDto appUserDto, string token)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1) { return await Task.FromResult(false); }
            if (role == "Admin")
            {
                var user = _userManager.FindByEmailAsync(appUserDto.AppUserName).Result;
                if (user == null) { return await Task.FromResult(false); }

                if (appUserDto.Type == "Client")
                {
                    var client = _context.Clients.FirstOrDefault(c => c.AppUser == user);
                    if (client == null) { return await Task.FromResult(false); }
                    _context.Addresses.Remove(_context.Addresses.FirstOrDefault(a => a.AppUserName == user.Id));
                    _context.Clients.Remove(client);

                    await _userManager.DeleteAsync(user);
                    await _context.SaveChangesAsync();

                    return await Task.FromResult(true);
                }
                else if (appUserDto.Type == "Provider")
                {
                    var address = _context.Addresses.FirstOrDefault(a => a.AppUserName == user.Id);
                    var provider = _context.Providers.FirstOrDefault(p => p.AppUser == user);

                    _context.Addresses.Remove(address);
                    _context.Providers.Remove(provider);

                    await _userManager.DeleteAsync(user);
                    await _context.SaveChangesAsync();
                    return await Task.FromResult(true);
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
            else
            {
                throw new Exception("Invalid role");
            }


        }

        public Task<List<AppUserDto>> GetAllUsers(string token)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }
            if (role == "Admin")
            {
                var users = _userManager.Users.Where(u => u.Id != userId).ToList();


                //var users = _userManager.Users.ToList();
                List<AppUserDto> list = new List<AppUserDto>();
                foreach (var user in users)
                {
                    list.Add(new AppUserDto
                    {
                        AppUserName = user.UserName,
                        Type = _userManager.GetRolesAsync(user).Result[0]
                    });
                }
                return Task.FromResult(list);
            }
            else
            {
                throw new Exception("Invalid role");
            }

        }

        //=======================================================================================================
        //                                  Categories  
        //=======================================================================================================

        public async Task<List<Category>> GetCategoriesAsync()
        {

            var categories = await _context.Categories.ToListAsync();

            if (categories == null)
            {
                return null;
            }

            return categories;
        }

        public async Task<Category> AddCategoryAsync(string token, Category categorydto)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }
            if (role == "Admin")
            {
                if(categorydto == null)
                {
                    return null;
                }
                var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.ServiceName == categorydto.ServiceName);
                if (categoryExists != null)
                {
                    return null;
                }
                var category = new Category
                {
                    ServiceName = categorydto.ServiceName,
                    Division = categorydto.Division,
                    Description = categorydto.Description,
                    ImageURL = categorydto.ImageURL
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return category;
            }
            else
            {
                throw new Exception("Invalid role");

            }

        }

        public async Task<bool> UpdateCategoryAsync(string token, int CategoryId, Category category)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return false;
            }
            if (role == "Admin")
            {
                var currCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == CategoryId);
                if (category == null)
                {
                    return false;
                }
                currCategory.ServiceName = category.ServiceName;
                currCategory.Division = category.Division;
                currCategory.Description = category.Description;
                currCategory.ImageURL = category.ImageURL;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new Exception("Invalid role");
            }
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteCategoryAsync(string token, int id)
        {
            (string userId, int Id, string role) = DecodeToken(token);
            if (Id == -1)
            {
                return false;
            }

            if (role == "Admin")
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                {
                    return false;
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new Exception("Invalid role");
            }

        }


        public async Task<List<Category>> SetCategorySubscriptions(string token, SubscriptionDto subscriptionDto)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }

            if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                if (provider == null)
                {
                    return null;
                }

                var providerCategories = _context.ProviderCategories.Where(pc => pc.ProviderId == provider.Id).ToList();
                if (providerCategories != null)
                {
                    _context.ProviderCategories.RemoveRange(providerCategories);
                }

                foreach (var categoryId in subscriptionDto.categories)
                {
                    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
                    if (category == null)
                    {
                        continue;
                    }
                    _context.ProviderCategories.Add(new ProviderCategory
                    {
                        ProviderId = provider.Id,
                        CategoryId = category.Id
                    });

                }

                await _context.SaveChangesAsync();
                var providerCategoriesList = _context.ProviderCategories.Where(pc => pc.ProviderId == provider.Id).ToList();
                if (providerCategoriesList != null)
                {
                    return providerCategoriesList.Select(pc => _context.Categories.FirstOrDefault(c => c.Id == pc.CategoryId)).ToList();
                }
                return null;


            }
            else
            {
                return null;
            }
        }

        public async Task<List<Category>> GetCategorySubscriptions(string token)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }

            if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                if (provider == null)
                {
                    return null;
                }
                var providerCategoriesList = _context.ProviderCategories.Where(pc => pc.ProviderId == provider.Id).ToList();
                if (providerCategoriesList != null)
                {
                    return providerCategoriesList.Select(pc => _context.Categories.FirstOrDefault(c => c.Id == pc.CategoryId)).ToList();
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        public async Task<bool> AddCategoryToProviderAsync(string token, List<int> categories)
        {
            (string userId, int id, string role) = DecodeToken(token);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null)
            {
                return false;
            }

            foreach (var categoryId in categories)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                {
                    continue;
                }
                var providerCategory = await _context.ProviderCategories.FirstOrDefaultAsync(pc => pc.ProviderId == provider.Id && pc.CategoryId == category.Id);
                if (providerCategory != null)
                {
                    continue;
                }
                _context.ProviderCategories.Add(new ProviderCategory
                {
                    ProviderId = provider.Id,
                    CategoryId = category.Id
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RemoveCategoryFromProviderAsync(string token, List<int> categories)
        {
            (string userId, int id, string role) = DecodeToken(token);
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null)
            {
                return false;
            }

            foreach (var categoryId in categories)
            {
                var providerCategory = await _context.ProviderCategories.FirstOrDefaultAsync(pc => pc.ProviderId == provider.Id && pc.CategoryId == categoryId);

                if (providerCategory == null)
                {
                    continue;
                }
                _context.ProviderCategories.Remove(providerCategory);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<ProviderDto>> GetProvidersByCategoryIdAsync(int id)
        {
            var categories = _context.ProviderCategories.Where(pc => pc.CategoryId == id).ToList();
            List<ProviderDto> providers = new List<ProviderDto>();
            foreach (var category in categories)
            {
                var provider = _context.Providers.FirstOrDefault(p => p.Id == category.ProviderId);
                var address = _context.Addresses.FirstOrDefault(a => a.AppUserName == provider.AppUserName);
                var categoriesList = getProviderCategories(provider.Id);
                providers.Add(new ProviderDto
                {
                    Name = provider.FirstName + ", " + provider.LastName,
                    Bio = provider.Bio,
                    DateJoined = provider.CreatedAt.ToString(),
                    City = address.City,
                    //Rating = provider.Rating.ToString(),
                    Image = provider.Image,
                    Services = categoriesList
                });
            }
            return Task.FromResult(providers);
        }

        private List<CategoryDtos> getProviderCategories(int id)
        {
            var pCategories = _context.ProviderCategories.Where(pc => pc.ProviderId == id).ToList();
            if (pCategories == null)
            {
                return null;
            }
            var categoriesList = new List<CategoryDtos>();
            foreach (var pc in pCategories)
            {
                var c = _context.Categories.FirstOrDefault(c => c.Id == pc.CategoryId);
                if (c == null)
                {
                    continue;
                }
                categoriesList.Add(new CategoryDtos
                {
                    Service = c.ServiceName,
                    Category = c.Division,
                    Description = c.Description,
                    Image = c.ImageURL
                });
            }
            return categoriesList;
        }


        //=======================================================================================================
        //                                  Booking
        //=======================================================================================================
        /*
         * 
         * Booking Algorithm:
         * 1. VerifyAddress : Ensure the address provided by the customer is valid and within the service area.
         * 2. NotifyLocalProviders : Notify the local providers of the new booking request. The providers are notified based on the category of the service and their proximity to the client's address.
         * 3. CollectResponses: Collect responses from the notified providers. The providers can either accept or reject the booking request.
         * 4. ConfirmBooking: This step involves confirming the booking with the selected provider.
         * 5. CompleteBooking: This step involves, NotifyClient & completing the booking process.
         * 
         */
        public async Task<object> CreateBookingAsync(string token, BookingDto bookingDto)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }

            if (role == "Client")
            {
                Address address = null;
                var client = _context.Clients.FirstOrDefault(c => c.Id == id);
                var category = _context.Categories.FirstOrDefault(c => c.Id == bookingDto.CategoryId);
                if (bookingDto.Address is null)
                {
                    address = _context.Addresses.FirstOrDefault(a => a.AppUserName == client.AppUserName);
                }
                else
                {
                    var dbAddress = _context.Addresses.FirstOrDefault(a => a.StreetAddress == bookingDto.Address.StreetAddress && a.PostalCode == bookingDto.Address.PostalCode);
                    if (dbAddress == null)
                    {
                        address = VerifyAddress(bookingDto.Address);
                    
                        if (address is null) return null;
                        _context.Addresses.Add(address);
                        _context.SaveChanges();
                    }
                    else
                    {
                        address = dbAddress;
                    }
                }


                var booking = new Booking
                {
                    ServiceDate = bookingDto.ServiceDate,
                    BookingDate = bookingDto.BookingDate,
                    Status = bookingDto.Status,
                    CategoryId = bookingDto.CategoryId,
                    ClientId = id,
                    //Provider Id is set by the CollectResponses function
                    AddressId = address.AddressId,
                    Notes = bookingDto.Notes,
                    UpdatedAt = DateTime.Now,
                };
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                await notifyLocalProviders(client.Id, address, category, booking.Id);

                var BookingDetails = new
                {
                    id = booking.Id,
                    serviceDate = booking.ServiceDate,
                    bookingDate = booking.BookingDate,
                    updatedAt = booking.UpdatedAt,
                    status = booking.Status,
                    clientId = booking.ClientId,
                    categoryId = booking.CategoryId,
                    addressId = booking.AddressId,
                    notes = booking.Notes,
                    address = new
                    {
                        streetAddress = address.StreetAddress,
                        city = address.City,
                        postalCode = address.PostalCode,
                        country = address.Country
                    },
                    category = new
                    {
                        id = category.Id,
                        service = category.ServiceName,
                        category = category.Division,
                        description = category.Description,
                        image = category.ImageURL
                    },
                    client = new
                    {
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        email = _context.Users.FirstOrDefault(u => u.Id == client.AppUserName)?.Email,
                        phone = client.Phone
                    },
                };

                return BookingDetails;
               

            }
            else
            {
                throw new Exception("Invalid role");
            }
        }

        private Address VerifyAddress(AddressDto addressDto)
        {
            
            var coords = getCoords(addressDto);
            var lat = coords.Result.Item1;
            var lon = coords.Result.Item2;
            if (lat == -1 || lon == -1)
            {
                return null;
            }
            return new Address
            {
                StreetAddress = addressDto.StreetAddress,
                City = addressDto.City,
                PostalCode = addressDto.PostalCode,
                Country = addressDto.Country,
                Latitude = lat,
                Longitude = lon
            };

        }

        private async Task<bool> notifyLocalProviders(int clientId, Address address, Category category, int bookingId)
        {

            var providersId = _context.ProviderCategories.Where(pc => pc.CategoryId == category.Id).ToList();
            var providers = new List<Provider>();
            var booking = _context.Bookings.FirstOrDefault(b => b.Id == bookingId);
            foreach (var pc in providersId)
            {
                var provider = _context.Providers.FirstOrDefault(p => p.Id == pc.ProviderId);
                var providerAddress = _context.Addresses.FirstOrDefault(a => a.AppUserName == provider.AppUserName);
                providers.Add(provider);

            }

            if (providers.Count == 0)
            {
                return false;
            }

            if (providers.Count == 1)
            {
                var provider = providers[0];

                var notificationDto = new NotificationDto
                {
                    Title = $"A New Booking Request for {category.ServiceName}",
                    Body = $"Hello {provider.FirstName},\n\nA new booking request for the {category.ServiceName} category has arrived, located at {address.PostalCode}, {address.City}. The requested service date is {booking.ServiceDate}",
                    Action = "Created",
                    ProviderId = provider.Id,
                    Data = new { ClientId = clientId, AddressId = address.AddressId, CategoryId = category.Id, BookingId = bookingId },
                    Type = NotificationType.Booking,
                    CreatedAt = DateTime.Now

                };
                await CreateNotification("Provider", notificationDto);
                var notifiedProvider = new NotifiedProvider
                {
                    ProviderId = provider.Id,
                    BookingId = bookingId,
                    UpdatedAt = DateTime.Now,
                    NotifiedAt = DateTime.Now,
                    Status = "Notified",
                };
                _context.NotifiedProviders.Add(notifiedProvider);
                _context.SaveChanges();

                var notificationDtoClient = new NotificationDto
                {
                    Title = $"Booking Request for {category.ServiceName} has been sent",
                    Body = $"Your booking request has been sent to local providers who offer {category.ServiceName} around {address.PostalCode} {address.City}, {address.Province}. The requested service date is {booking.ServiceDate}",
                    Action = "Created",
                    ClientId = clientId,
                    Data = new { ClientId = clientId, AddressId = address.AddressId, CategoryId = category.Id, BookingId = bookingId },
                    Type = NotificationType.Alert,
                    CreatedAt = DateTime.Now
                };
                await CreateNotification("Client", notificationDtoClient);


                return true;
            }

            //sorting the provider by distance to address of the booking
            providers.Sort((p1, p2) =>
            {
                var p1Address = _context.Addresses.FirstOrDefault(a => a.AppUserName == p1.AppUserName);
                var p2Address = _context.Addresses.FirstOrDefault(a => a.AppUserName == p2.AppUserName);
                return distance(address, p1Address).CompareTo(distance(address, p2Address));
            });


            //The top 4 closest providers
            if (providers.Count > 4)
            {
                providers = (List<Provider>)providers.Take(4).ToList();
            }

            foreach (var provider in providers)
            {
                var notificationDto = new NotificationDto
                {
                    Title = $"A New Booking Request for {category.ServiceName}",
                    Body = $"Hello {provider.FirstName},\n\nA new booking request for the {category.ServiceName} category has arrived, located at {address.PostalCode}, {address.City}. The requested service date is {booking.ServiceDate}",
                    Action = "Created",
                    ProviderId = provider.Id,
                    Data = new { ClientId = clientId, AddressId = address.AddressId, CategoryId = category.Id, BookingId = bookingId },
                    Type = NotificationType.Booking,
                    CreatedAt = DateTime.Now

                };
                await CreateNotification("Provider", notificationDto);
                var notifiedProvider = new NotifiedProvider
                {
                    ProviderId = provider.Id,
                    BookingId = bookingId,
                    UpdatedAt = DateTime.Now,
                    NotifiedAt = DateTime.Now,
                    Status = "Notified",
                };
                _context.NotifiedProviders.Add(notifiedProvider);
                _context.SaveChanges();


            }

            var clientNtf = new NotificationDto
            {
                Title = $"Booking Request for {category.ServiceName} has been sent",
                Body = $"Your booking request has been sent to local providers who offer {category.ServiceName} around {address.PostalCode} {address.City}, {address.Province}. The requested service date is  {booking.ServiceDate}",
                Action = "Created",
                ClientId = clientId,
                Data = new { ClientId = clientId, AddressId = address.AddressId, CategoryId = category.Id, BookingId = bookingId },
                Type = NotificationType.Alert,
                CreatedAt = DateTime.Now
            };
            await CreateNotification("Client", clientNtf);



            return true;

        }

        private double distance(Address address, Address p1Address)
        {
            try
            {
                var subscriptionKey = "pUBV4lwanhEEiGQZSgR4KH_dqMv5QjunNm8A2YIwS1c";
                string url = $"https://atlas.microsoft.com/route/directions/matrix/json?api-version=1.0&subscription-key=" + subscriptionKey;

                var requestData = new
                {
                    origins = new[] { p1Address.StreetAddress },
                    destinations = new[] { address.StreetAddress }
                };
                HttpClient httpClient = new HttpClient();
                var response = httpClient.PostAsJsonAsync(url, requestData);

                var responseBody = response.Result.Content.ToString();
                var responseData = JObject.Parse(responseBody);

                if (responseData["error"] != null)
                {
                    Console.WriteLine("Error: " + responseData["error"]["message"]);
                    return -1;
                }

                double distance = responseData["results"][0]["results"][0]["travelDistance"].Value<double>();
                return distance;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        public async Task<object> GetBookingDetailsAsync(string token, int bookingId)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
            {
                return null;
            }
            var address = _context.Addresses.FirstOrDefault(a => a.AddressId == booking.AddressId);
            var category = _context.Categories.FirstOrDefault(c => c.Id == booking.CategoryId);
            try
            {
                if (role == "Provider")
                {
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == booking.ClientId);
                    var BookingDetails = new
                    {
                        id = booking.Id,
                        serviceDate = booking.ServiceDate,
                        bookingDate = booking.BookingDate,
                        updatedAt = booking.UpdatedAt,
                        status = booking.Status,
                        providerId = booking.ProviderId,
                        clientId = booking.ClientId,
                        categoryId = booking.CategoryId,
                        addressId = booking.AddressId,
                        notes = booking.Notes,
                        address = new
                        {
                            streetAddress = address.StreetAddress,
                            city = address.City,
                            postalCode = address.PostalCode,
                            country = address.Country
                        },
                        category = new
                        {
                            id = category.Id,
                            service = category.ServiceName,
                            category = category.Division,
                            description = category.Description,
                            image = category.ImageURL
                        },
                        client = new
                        {
                            firstName = client.FirstName,
                            lastName = client.LastName,
                            email = _context.Users.FirstOrDefault(u => u.Id == client.AppUserName)?.Email,
                            phone = client.Phone
                        },
                    };


                    return BookingDetails;
                }
                else if (role == "Client")
                {
                    var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == booking.ProviderId);
                    if (provider == null)
                    {
                        provider = new Provider
                        {
                            FirstName = "Not Assigned",
                            LastName = "Not Assigned",
                            Phone = "N/A",
                            AppUserName = "Not Assigned"
                        };
                    }
                    var BookingDetails = new
                    {
                        id = booking.Id,
                        serviceDate = booking.ServiceDate,
                        bookingDate = booking.BookingDate,
                        updatedAt = booking.UpdatedAt,
                        status = booking.Status,
                        providerId = booking.ProviderId,
                        clientId = booking.ClientId,
                        categoryId = booking.CategoryId,
                        addressId = booking.AddressId,
                        notes = booking.Notes,
                        address = new
                        {
                            streetAddress = address.StreetAddress,
                            city = address.City,
                            postalCode = address.PostalCode,
                            country = address.Country
                        },
                        category = new
                        {
                            id = category.Id,
                            service = category.ServiceName,
                            category = category.Division,
                            description = category.Description,
                            image = category.ImageURL
                        },
                        provider = new
                        {
                            firstName = provider.FirstName,
                            lastName = provider.LastName,
                            email = _context.Users.FirstOrDefault(u => u.Id == provider.AppUserName)?.Email,
                            phone = provider.Phone
                        }

                    };
                    return BookingDetails;

                } else { return null; }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public async Task<int> UpdateBookingAsync(string token, int bookingId, bool? isConfirmed, BookingDto bookingDto)
        {
            (string userId, int id, string role) = DecodeToken(token);

            if (id == -1) { return 400; }
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) { return 400; }

            var address = _context.Addresses.FirstOrDefault(a => a.AddressId == booking.AddressId);
            if (role != "Provider" && bookingDto.Address is not null)
            {
                address = VerifyAddress(bookingDto.Address);
            }
            if (address is null) { return 400; }


            var category = _context.Categories.FirstOrDefault(c => c.Id == booking.CategoryId);



            if (role == "Client") //update the booking from the bookingDto
            {
                booking.UpdatedAt = DateTime.Now;
                booking.ServiceDate = bookingDto.ServiceDate;
                booking.BookingDate = DateTime.Now;
                booking.Notes = bookingDto.Notes;
                booking.Address = address;

                if(booking.Status == "Confirmed")
                {
                    var provider = _context.Providers.FirstOrDefault(p => p.Id == booking.ProviderId);
                    await CreateNotification("Provider", new NotificationDto
                    {
                        Title = $"Booking Updated for {category.ServiceName}",
                        Body = $"Hi {provider.FirstName}, the booking for {category.ServiceName} on {bookingDto.ServiceDate} has been updated by the client" +
                        $" located in {address.PostalCode}, {address.City}. Kindly confirm or decline when you get a moment. Thanks!",
                        Action = "Reminder",
                        Type = NotificationType.Reminder,
                        ProviderId = booking.ProviderId,
                        Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Type = NotificationType.Booking, Action = "Reminder" },
                        CreatedAt = DateTime.Now
                    });
                } else { 
                //notify the providers of the booking update
                var notifiedProviders = _context.NotifiedProviders.Where(np => np.BookingId == booking.Id).ToList();
                if (notifiedProviders.Count == 0)
                {
                    await notifyLocalProviders((int)booking.ClientId, address, category, booking.Id);
                }
                else
                {
                    foreach (var np in notifiedProviders)
                    {
                        np.UpdatedAt = DateTime.Now;
                        np.Status = "Notified";
                    }
                    foreach (var np in notifiedProviders)
                    {
                        var provider = _context.Providers.FirstOrDefault(p => p.Id == np.ProviderId);
                        await CreateNotification("Provider", new NotificationDto
                        {
                            Title = $"Booking Updated for {category.ServiceName}",
                            Body = $"Hi {provider.FirstName}, the booking for {category.ServiceName} on {bookingDto.ServiceDate} has been updated by the client" +
                            $" located in {address.PostalCode}, {address.City}. Kindly confirm or decline when you get a moment. Thanks!",
                            Action = "Reminder",
                            Type = NotificationType.Reminder,
                            ProviderId = np.ProviderId,
                            Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Type = NotificationType.Booking, Action = "Reminder" },
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                }
                _context.Update(booking);
                _context.SaveChanges();
                return 200;
            }
            else if (role == "Provider") // CollectResponses
            {
                return await ConfirmBooking((bool)isConfirmed, booking, id);
            }
            else if (role == "Admin")
            {
                booking.Status = "Pending";
                booking.UpdatedAt = DateTime.Now;
                booking.ServiceDate = bookingDto.ServiceDate;
                booking.BookingDate = bookingDto.BookingDate;
                booking.Notes = bookingDto.Notes;
                booking.CategoryId = bookingDto.CategoryId;

                //notify the provider and the client of the booking update
                await CreateNotification("Client", new NotificationDto
                {
                    Title = "Booking Updated",
                    Body = "Your booking has been updated to the following:\n " + JsonConvert.SerializeObject(booking),
                    Action = "Updated",
                    Type = NotificationType.Booking,
                    ClientId = booking.ClientId,
                    Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                    CreatedAt = DateTime.Now
                });

                var notifiedProviders = _context.NotifiedProviders.Where(np => np.BookingId == booking.Id).ToList();
                if (notifiedProviders.Count == 0)
                {
                    await notifyLocalProviders((int)booking.ClientId, address, category, booking.Id);
                }
                else
                {
                    foreach (var np in notifiedProviders)
                    {
                        np.UpdatedAt = DateTime.Now;
                        np.Status = "Notified";
                    }
                    foreach (var np in notifiedProviders)
                    {
                        var provider = _context.Providers.FirstOrDefault(p => p.Id == np.ProviderId);
                        await CreateNotification("Provider", new NotificationDto
                        {
                            Title = $"Booking Updated for {category.ServiceName}",
                            Body = $"Hi {provider.FirstName}, the booking for {category.ServiceName} on {bookingDto.ServiceDate} has been updated by the client" +
                            $" located in {address.PostalCode}, {address.City}. Kindly confirm or decline when you get a moment. Thanks!",
                            Action = "Reminder",
                            Type = NotificationType.Alert,
                            ProviderId = np.ProviderId,
                            Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Action = "Reminder" },
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                _context.Update(booking);
                _context.SaveChanges();

                return 200;
            }
            else
            {
                throw new Exception("Invalid role");
            }
            throw new NotImplementedException();
        }
        /**
         * Booking Codes:
         * 150: Booking is already confirmed by this provider
         * 200: Booking is confirmed
         * 300: Booking is already confirmed by another provider
         * 400: Invalid Request
         */
        public async Task<int> ConfirmBooking(bool isConfirmed, Booking booking, int providerId)
        {
            var notifiedProviders = _context.NotifiedProviders.Where(np => np.BookingId == booking.Id).ToList();
            var serviceName = _context.Categories.FirstOrDefault(c => c.Id == booking.CategoryId).ServiceName;
            var postalCode = _context.Addresses.FirstOrDefault(c => c.AddressId == booking.AddressId).PostalCode;
            if (isConfirmed)
            {
                //if the booking is already confirmed and the provider is not the one who will service it
                if (booking.Status == "Confirmed")
                {
                    if (booking.ProviderId != providerId)
                    {
                        return 300; //Booking is already confirmed by another provider
                    }
                    return 150; //Booking is already confirmed by this provider
                }

                //Confirm the booking
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.Now;
                booking.ProviderId = providerId;


                foreach (var np in notifiedProviders)
                {
                    if (np.ProviderId != providerId)
                    {
                        _context.NotifiedProviders.Remove(np);
                    }
                }


                //Notify the client of the booking confirmation
                await CreateNotification("Client", new NotificationDto
                {
                    Title = "Booking Confirmed",
                    Body = "Your booking has been confirmed:\n",
                    Action = "Confirmed",
                    Type = NotificationType.Booking,
                    ClientId = booking.ClientId,
                    Data = new { BookingId = booking.Id, ProviderId = providerId, Type = NotificationType.Booking, Action = "Confirmed" },
                    CreatedAt = DateTime.Now
                });

                //Notify the provider of the booking confirmation
                await CreateNotification("Provider", new NotificationDto
                {
                    Title = "Booking Confirmation",
                    Body = $"You have accepted the booking request for {serviceName} on {booking.ServiceDate}. Thank you for your confirmation.",
                    Action = "Confirmed",
                    Type = NotificationType.Reminder,
                    ProviderId = providerId,
                    Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Type = NotificationType.Booking, Action = "Confirmed" },
                    CreatedAt = DateTime.Now,
                });
                await _context.SaveChangesAsync();

            }
            else // if the provider rejects the booking
            {
                var notifiedProvider = notifiedProviders.FirstOrDefault(np => np.ProviderId == providerId);
                if (notifiedProvider == null) { return 100; /*Already Declined*/ }
                _context.NotifiedProviders.Remove(notifiedProvider);
                notifiedProviders.Remove(notifiedProvider);

                //if that was the last provider to reject the booking, then the whole booking itself is rejected
                if (notifiedProviders.Count == 0)
                {
                    booking.Status = "Pending";
                    booking.UpdatedAt = DateTime.Now;
                    await CreateNotification("Client", new NotificationDto
                    {
                        Title = $"Booking Rejected",
                        Body = $"There are no provider near {postalCode} that provide {serviceName} service on {booking.ServiceDate}!",
                        Action = "Rejected",
                        Type = NotificationType.Alert,
                        ClientId = booking.ClientId,
                        Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                        CreatedAt = DateTime.Now
                    });
                }
                else // if there are still providers to respond to the booking
                {
                    //Update the notified providers
                    foreach (var np in notifiedProviders)
                    {
                        np.UpdatedAt = DateTime.Now;
                        np.Status = "Notified";
                    }
                    //send a notification to the providers who are still notified
                    foreach (var np in notifiedProviders)
                    {
                        var provider = _context.Providers.FirstOrDefault(p => p.Id == np.ProviderId);
                        await CreateNotification("Provider", new NotificationDto
                        {
                            Title = $"Booking Request for {serviceName}",
                            Body = $"Hi {provider.FirstName}, just a friendly reminder about the booking request for {serviceName} on {booking.ServiceDate}. Kindly confirm or decline when you get a moment. Thanks!",
                            Action = "Reminder",
                            Type = NotificationType.Reminder,
                            ProviderId = np.ProviderId,
                            Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                            CreatedAt = DateTime.Now
                        });
                    }


                }
                await _context.SaveChangesAsync();
                return 200; //rejected
            }

            return 200;
        }

        public async Task<List<object>> GetUserBookingsAsync(string token)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }

            var bookings = new List<Booking>();
            var list = new List<object>();
            if (role == "Client")
            {
                bookings = _context.Bookings.Where(b => b.ClientId == id).ToList();
            }
            else
            if (role == "Provider")
            {
                bookings = _context.Bookings.Where(b => b.ProviderId == id).ToList();
            }

            if (bookings.Count == 0)
            {
                return list;
            }

            
            foreach (var booking in bookings)
            {
                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == booking.AddressId);
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == booking.CategoryId);
                if (role == "Provider")
                {
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == booking.ClientId);
                    if (client is null) 
                        continue;
                    var bookingDetails = new
                    {
                        id = booking.Id,
                        serviceDate = booking.ServiceDate,
                        bookingDate = booking.BookingDate,
                        updatedAt = booking.UpdatedAt,
                        status = booking.Status,
                        providerId = booking.ProviderId,
                        clientId = booking.ClientId,
                        categoryId = booking.CategoryId,
                        addressId = booking.AddressId,
                        notes = booking.Notes,
                        address = new
                        {
                            streetAddress = address.StreetAddress,
                            city = address.City,
                            postalCode = address.PostalCode,
                            country = address.Country
                        },
                        category = new
                        {
                            id = category.Id,
                            service = category.ServiceName,
                            category = category.Division,
                            description = category.Description,
                            image = category.ImageURL
                        },
                        client = new
                        {
                            firstName = client.FirstName,
                            lastName = client.LastName,
                            email = _context.Users.FirstOrDefault(u => u.Id == client.AppUserName)?.Email,
                            phone = client.Phone
                        },
                    };
                    list.Add(bookingDetails);
                }
                else if (role == "Client")
                {
                    var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == booking.ProviderId);
                    //var providerAddress = _context.Addresses.FirstOrDefault(a => a.AppUserName == provider.AppUserName);
                    if (provider is not null)
                        Console.WriteLine(provider.FirstName);
                    var bookingDetails = new
                    {
                        id = booking.Id,
                        serviceDate = booking.ServiceDate,
                        bookingDate = booking.BookingDate,
                        updatedAt = booking.UpdatedAt,
                        status = booking.Status,
                        providerId = booking.ProviderId,
                        clientId = booking.ClientId,
                        categoryId = booking.CategoryId,
                        addressId = booking.AddressId,
                        notes = booking.Notes,
                        address = new
                        {
                            streetAddress = address.StreetAddress,
                            city = address.City,
                            postalCode = address.PostalCode,
                            country = address.Country
                        },
                        category = new
                        {
                            id = category.Id,
                            service = category.ServiceName,
                            category = category.Division,
                            description = category.Description,
                            image = category.ImageURL
                        },
                        provider = new
                        {
                            firstName = provider?.FirstName,
                            lastName = provider?.LastName,
                            email = provider == null ? null : _context.Users.FirstOrDefault(u => u.Id == provider.AppUserName)?.Email,
                            phone = provider?.Phone
                        },
                    };

                    list.Add(bookingDetails);
                }
                
            }
            return list;
        }
        
        public async Task<bool> CancelBookingAsync(string token, int bookingId)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return false;
            }

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == booking.CategoryId);
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == booking.AddressId);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == booking.ClientId);
            if (booking == null)
            {
                return false;
            }

            if (role == "Client")
            {
                if (booking.Status == "Confirmed")
                {
                    return false; //Booking is already confirmed
                }

                if (booking.ProviderId != null)
                {
                    var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == booking.ProviderId);
                    await CreateNotification("Provider", new NotificationDto
                    {
                        Title = $"{category.ServiceName} Booking has been cancelled",
                        Body = $"The booking for {category.ServiceName} on {booking.ServiceDate} has been cancelled by the client {client.FirstName}",
                        Action = "Cancelled",
                        Type = NotificationType.Alert,
                        ProviderId = (int)booking.ProviderId,
                        Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Type = NotificationType.Alert, Action = "Cancelled" },
                        CreatedAt = DateTime.Now
                    });;
                }
                //notify the client
                await CreateNotification("Client", new NotificationDto
                {
                    Title = "Booking Cancelled",
                    Body = "You have cancelled the booking",
                    Action = "Cancelled",
                    Type = NotificationType.Alert,
                    ClientId = booking.ClientId,
                    Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                    CreatedAt = DateTime.Now
                });
                //delete the booking
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
                return true;
            }
            else if (role == "Provider")
            {
                
                booking.Status = "Pending";
                booking.UpdatedAt = DateTime.Now;
                booking.ProviderId = null;
                
                var notifiedProviders = _context.NotifiedProviders.Where(np => np.BookingId == booking.Id).ToList();

                if (notifiedProviders.Count == 0)
                {
                    await notifyLocalProviders((int)booking.ClientId, address, category, booking.Id);
                }
                else
                {
                    foreach (var np in notifiedProviders)
                    {
                        if (np.ProviderId == id)
                        {
                            _context.NotifiedProviders.Remove(np);
                            continue;
                        }
                        else { 
                        np.UpdatedAt = DateTime.Now;
                        np.Status = "Notified";
                        var provider = _context.Providers.FirstOrDefault(p => p.Id == np.ProviderId);
                        await CreateNotification("Provider", new NotificationDto
                        {
                            Title = $"Booking Request for {category.ServiceName}",
                            Body = $"Hi {provider.FirstName}, just a friendly reminder about the booking request for {category.ServiceName} on {booking.ServiceDate}. Kindly confirm or decline when you get a moment. Thanks!",
                            Action = "Reminder",
                            Type = NotificationType.Booking,
                            ProviderId = np.ProviderId,
                            Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                            CreatedAt = DateTime.Now
                        });
                            }
                    }
                    await CreateNotification("Provider", new NotificationDto
                    {
                        Title = $"{booking.Category.ServiceName} Booking Cancelled",
                        Body = "You have cancelled the booking",
                        Action = "Cancelled",
                        Type = NotificationType.Alert,
                        ProviderId = id,
                        Data = new { BookingId = booking.Id, ClientId = booking.ClientId, Type = NotificationType.Alert, Action = "Cancelled" },
                        CreatedAt = DateTime.Now
                    });

                    //send a notification to the client
                    await CreateNotification("Client", new NotificationDto
                    {
                        Title = $"{booking.Category.ServiceName} Booking Cancelled by the provider",
                        Body = "The provider has cancelled the booking, searching for other providers",
                        Action = "Cancelled",
                        Type = NotificationType.Alert,
                        ClientId = booking.ClientId,
                        Data = new { BookingId = booking.Id, AddressId = booking.AddressId, CategoryId = booking.CategoryId },
                        CreatedAt = DateTime.Now
                    });
                   
                    
                }
                _context.Update(booking);
                _context.SaveChanges();
                return true;

            }
            return false;
        }

        }
    }
