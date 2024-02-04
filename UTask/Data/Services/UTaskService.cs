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

namespace UTask.Data.Services
{
    public class UTaskService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UTaskService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
           SignInManager<IdentityUser> signInManager
           , IConfiguration config, IEmailSender emailSender, UTaskDbContext context)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _config = config;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        private (string, int, string) DecodeToken(string token)
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
        //=======================================================================================================
        //                                  Authroization and Authentication
        //=======================================================================================================
        public async Task<IdentityResult> RegisterUserAsync(RegisterationDto rdto)
        {
            
            if (!await _roleManager.RoleExistsAsync(rdto.Type))
            {
                return null;
            }
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
                    AppUserName = appUser.UserName,
                    AppUser = appUser
                });
                _context.SaveChanges();

                var address = _context.Addresses.FirstOrDefault(a => a.City == rdto.Address.City && a.Country == rdto.Address.Country && a.StreetAddress == rdto.Address.StreetAddress && a.PostalCode == rdto.Address.PostalCode);
                // TODO END // 
                if (rdto.Type == "Client")
                {
                    
                    _context.Clients.Add(new Client { 
                        AppUser = appUser, 
                        AppUserName = appUser.UserName, 
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName
                    });
                      
                }
                else if (rdto.Type == "Provider")
                {
                    var provider = _context.Providers.Add(new Provider { 
                        AppUser = appUser, 
                        AppUserName = appUser.UserName,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName
                    });
                    Console.WriteLine(provider.Entity.Id);
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

        public async Task<string> LoginUserAsync(LoginDto ldto)
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

                        if (client == null)
                        {
                            return null;
                        }
                        
                        
                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], client.Id),
                            role = role,
                            user = new
                            {
                                firstName = client.FirstName,
                                lastName = client.LastName,
                                address = new
                                {
                                    streetAddress = address.StreetAddress,
                                    city = address.City,
                                    postalCode = address.PostalCode,
                                    country = address.Country
                                }
                    }
                        };

                        return JsonConvert.SerializeObject(response);

                    }
                       
                    else
                    if (role[0] == "Admin") {
                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], 0),
                            role = role,
                            user = new { Email = user.Email }
                        };

                        return JsonConvert.SerializeObject(response);
                    } 
                    
                    else
                    {
                        var provider = await _context.Providers.FirstOrDefaultAsync(p => p.AppUser == user);
                        if (provider == null)
                        {
                            return null;
                        }

                        var response = new
                        {
                            token = GenerateTokenString(user.Id, role[0], provider.Id),
                            role = role,
                            user = new
                            {
                                firstName = provider.FirstName,
                                lastName = provider.LastName,
                                address = new
                                {
                                    streetAddress = address.StreetAddress,
                                    city = address.City,
                                    postalCode = address.PostalCode,
                                    country = address.Country
                                }
                            }
                        };

                        return JsonConvert.SerializeObject(response);
                    }
                }
                return null;
                
            }

            return null;
        }

        public string GenerateTokenString(string userId, string role, int Id)
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
        public async Task<string> GetUser(string token)
        {
            
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return null;
            }
            var user = await _userManager.FindByIdAsync(userId);
            var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AppUserName == user.UserName);
            if (user == null)
            {
                   return null;
            }
            if (role == "Client")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
                
                if (client == null)
                {
                    return null;
                }
                var response = new
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    role = role,
                    Phone = client.Phone,
                    Email = user.Email,
                    Address = new AddressDto
                    {
                        StreetAddress = address.StreetAddress,
                        City = address.City,
                        PostalCode = address.PostalCode,
                        Province = address.Province,
                        Country = address.Country
                    }
                };
                return JsonConvert.SerializeObject(response);
                
            }
            else if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                
                
                if (provider == null)
                {
                    return null;
                }
                var response = new
                {
                    FirstName = provider.FirstName,
                    LastName = provider.LastName,
                    role = role,
                    Phone = provider.Phone,
                    Email = user.Email,
                    Address = new AddressDto
                    {
                        StreetAddress = address.StreetAddress,
                        City = address.City,
                        PostalCode = address.PostalCode,
                        Province = address.Province,
                        Country = address.Country
                    }
                    
                    
                };
                return JsonConvert.SerializeObject(response);
            } else if (role == "Admin")
            {
                
                var response = new
                {
                    Email = user.Email,
                    role = role
                };
                return JsonConvert.SerializeObject(response);
            }
            else
            {
                throw new Exception("Invalid role");
            }   
        }

        public Task<bool> UpdateUser(ProfileDto updateUserDto, string token)
        {
            
            (string userId, int id, string role) = DecodeToken(token);
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || id == -1)
            {
                return Task.FromResult(false);
            }
            var address = _context.Addresses.FirstOrDefault(c => c.AppUserName == user.UserName);
            if (role == "Client")
            {
                var client = _context.Clients.FirstOrDefault(c => c.Id == id);

                if (client == null)
                {
                    return Task.FromResult(false);
                }

                client.FirstName = updateUserDto.FirstName;
                client.LastName = updateUserDto.LastName;
                client.Phone = updateUserDto.Phone;
                user.PhoneNumber = updateUserDto.Phone;
                address.StreetAddress = updateUserDto.Address.StreetAddress;
                address.City = updateUserDto.Address.City;
                address.PostalCode = updateUserDto.Address.PostalCode;
                address.Province = updateUserDto.Address.Province;
                address.Country = updateUserDto.Address.Country;

                _userManager.UpdateAsync(user);
                _context.SaveChangesAsync();
                return Task.FromResult(true);
            }
            else if (role == "Provider")
            {
                var provider = _context.Providers.FirstOrDefault(p => p.Id == id);
                if (provider == null)
                {
                    return Task.FromResult(false);
                }
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
            } else if (role == "Admin")
            {
                return Task.FromResult(true);
            }
            else
            {
                throw new Exception("Invalid role");
            }
        }
        
        public async Task<bool> DeleteAccount(DeleteUserDto deleteUserDto, string token)
        {

            (string userId, int id, string role) = DecodeToken(token);
            var user = await _userManager.FindByIdAsync(userId);
            if (id == -1 || user == null)
            {
                return false; // user not found
            }
            var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AppUserName == user.UserName);
            if (role == "Client")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
                
                
                _context.Clients.Remove(client);
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                return true;
            }
            else if (role == "Provider")
            {
                var provider = await _context.Providers.FirstOrDefaultAsync(p => p.Id == id);
                if (provider == null)
                {
                    return false;
                }
                _context.Providers.Remove(provider);
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                return true;
            }
            else if (role == "Admin")
            {
                    await _userManager.DeleteAsync(user);
                return true;
            } else {
                   throw new Exception("Invalid role");
            }
        }
        //                                          Admin Privileges

        public Task<bool> DeleteUser(AppUserDto appUserDto, string token)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return Task.FromResult(false);
            }
            if(role == "Admin")
            {
                var user = _userManager.FindByEmailAsync(appUserDto.AppUserName).Result;
                
                //delete user
                
                if (user == null)
                {
                    return Task.FromResult(false);
                }
                _userManager.DeleteAsync(user);
                if (appUserDto.Type == "Client")
                {
                    var client = _context.Clients.FirstOrDefault(c => c.AppUser == user);
                    if (client == null)
                    {
                        return Task.FromResult(false);
                    }
                    _context.Clients.Remove(client);
                    _context.Addresses.Remove(_context.Addresses.FirstOrDefault(a => a.AppUserName == user.UserName));
                    _context.SaveChanges();
                    _userManager.DeleteAsync(user);
                    return Task.FromResult(true);
                }
                else if (appUserDto.Type == "Provider")
                {
                    var provider = _context.Providers.FirstOrDefault(p => p.AppUser == user);
                    if (provider == null)
                    {
                        return Task.FromResult(false);
                    }
                    _context.Providers.Remove(provider);
                    _context.Addresses.Remove(_context.Addresses.FirstOrDefault(a => a.AppUserName == user.UserName));
                    _context.SaveChanges();
                    _userManager.DeleteAsync(user);
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);
                }
            } else
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

        public async Task<bool> AddCategoryAsync(string token, CategoryDtos categoryDto)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return false;
            }
            if (role == "Admin")
            {
                var category = new Category
                {
                    ServiceName = categoryDto.Service,
                    Division = categoryDto.Category,
                    Description = categoryDto.Description,
                    ImageURL = categoryDto.Image
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new Exception("Invalid role");
                
            }
            
        }

        public async Task<bool> UpdateCategoryAsync(string token, int CategoryId, CategoryDtos categoryDto)
        {
            (string userId, int id, string role) = DecodeToken(token);
            if (id == -1)
            {
                return false;
            }
            if (role == "Admin")
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == CategoryId);
                if (category == null)
                {
                    return false;
                }
                category.ServiceName = categoryDto.Service;
                category.Division = categoryDto.Category;
                category.Description = categoryDto.Description;
                category.ImageURL = categoryDto.Image;
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


    }
}
