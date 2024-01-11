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

namespace UTask.Data.Services
{
    public class AuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UTaskDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
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

        public async Task<IdentityResult> RegisterUserAsync(RegisterationDto rdto)
        {
            var appUser = new AppUser
            {
                FirstName = rdto.FirstName,
                LastName = rdto.LastName,
                UserName = rdto.Email,
                Email = rdto.Email,
                Type = rdto.Type == "Client" ? UserType.Client : UserType.Provider
            };
            var result = await _userManager.CreateAsync(appUser, rdto.Password);
            await _userManager.AddToRoleAsync(appUser, rdto.Type);
            if (result.Succeeded)
            {


                _context.Addresses.Add(new Address
                {
                    StreetAddress = rdto.Address.StreetAddress,
                    City = rdto.Address.City,
                    PostalCode = rdto.Address.PostalCode,
                    Country = rdto.Address.Country
                });
                _context.SaveChanges();
                var address = _context.Addresses.FirstOrDefault(a => a.City == rdto.Address.City && a.Country == rdto.Address.Country && a.StreetAddress == rdto.Address.StreetAddress && a.PostalCode == rdto.Address.PostalCode);
                if (rdto.Type == "Client")
                {
                    
                    _context.Clients.Add(new Client { 
                        AppUser = appUser, 
                        AppUserName = appUser.UserName, 
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName,
                        AddressId = address.AddressId,
                        Address = address
                    });
                      
                }
                else
                {
                    _context.Providers.Add(new Provider { 
                        AppUser = appUser, 
                        AppUserName = appUser.UserName,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName,
                        AddressId = address.AddressId,
                        Address = address
                    });
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
                if (role != null)
                {
                       if (role[0] == "Client")
                    {
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.AppUser == user);

                        if (client == null)
                        {
                            return null;
                        }
                        var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AddressId == client.AddressId);
                        
                        var response = new
                        {
                            token = GenerateTokenString(user.UserName, role[0], client.Id),
                            role = role,
                            user = new
                            {
                                firstName = client.FirstName,
                                lastName = client.LastName,
                                address= address
                    }
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
                        var address = await _context.Addresses.FirstOrDefaultAsync(c => c.AddressId == provider.AddressId);

                        var response = new
                        {
                            token = GenerateTokenString(user.UserName, role[0], provider.Id),
                            role = role,
                            user = new
                            {
                                firstName = provider.FirstName,
                                lastName = provider.LastName,
                                address = address
                            }
                        };

                        return JsonConvert.SerializeObject(response);
                    }
                }
                return null;
                
            }

            return null;
        }

        public string GenerateTokenString(string username, string role, int Id)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
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
            //Decode the token
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
            //Use the SignOutAsync method of the _signInManager field to sign out the user
            await _signInManager.SignOutAsync();
            return true;
        }

/*        public async Task<bool> CreateRole(string role)
        {
            //create role from the role parameter using the _roleManager field
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            return result.Succeeded;
        }*/
    }
}
