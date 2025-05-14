using Core.DTO;
using Core.DTO.AccountingDTO;
using Core.Entities;
using Core.Helper;
using Core.Interfaces;
using IF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IF.Repositories
{
    public class AccountingRepository: IAccountingRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        public AccountingRepository(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IOptions<JwtSettings> options,
                           AppDbContext context,
                           IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = options.Value;
            _context = context;
            _userRepository = userRepository;
        }
        public async Task<AuthResultDTO> LogIn(LogInDTO logInDTO)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(logInDTO.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, logInDTO.Password))
            {
                return new AuthResultDTO { Message = "Email or Password is not correct" };
            }
            JwtSecurityToken JwtToken = await CreateToken(user);
            var refreshToken = new RefreshToken();
            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                refreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            }
            else
            {
                refreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                try
                {
                    await _userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    return new AuthResultDTO { Message = ex.Message };
                }
            }
            return new AuthResultDTO
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(JwtToken),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpired = refreshToken.ExpiredOn
            };
        }

        public async Task<AuthResultDTO> RegisteUser(RegisterDTO UserDTO)
        {
            if (await _userManager.FindByEmailAsync(UserDTO.Email) is not null)
            {
                return new AuthResultDTO() { Message = "Email is used." };
            }
            if (await _userManager.FindByNameAsync(UserDTO.UserName) is not null)
            {
                return new AuthResultDTO() { Message = "Username is used." };
            }
            var user = new ApplicationUser
            {
                Name = UserDTO.Name,
                Email = UserDTO.Email,
                PhoneNumber = UserDTO.PhoneNumber,
                UserName = UserDTO.UserName,
            };
            var refreshToken = new RefreshToken();
            var oldPath = "";
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _userManager.CreateAsync(user, UserDTO.Password);
                    if (!result.Succeeded)
                    {
                        var error = "";
                        foreach (var item in result.Errors)
                        {
                            error += " " + item.Description;
                        }
                        return new AuthResultDTO() { Message = error };
                    }
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        var error = "";
                        foreach (var item in roleResult.Errors)
                        {
                            error += " " + item.Description;
                        }
                        return new AuthResultDTO() { Message = error };
                    }
                    refreshToken = GenerateRefreshToken();
                    user.RefreshTokens.Add(refreshToken);
                    var addImageresult = await _userRepository.AddImage(UserDTO.Image, user.Id);
                    oldPath = addImageresult.Id;
                    if (!string.IsNullOrEmpty(addImageresult.Message))
                    {
                        return new AuthResultDTO { Message = addImageresult.Message };
                    }
                    await _userManager.UpdateAsync(user);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _userRepository.DeleteImage(oldPath);
                    return new AuthResultDTO() { Message = ex.Message };
                }
            }
            var token = await CreateToken(user);
            return new AuthResultDTO()
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpired = refreshToken.ExpiredOn,
            };
        }
        public async Task<AuthResultDTO> CheckRefreshTokenAndRevoke(string token)
        {
            var user = _userManager.Users.SingleOrDefault(x => x.RefreshTokens.Any(t => t.Token == token));
            if (user == null)
            {
                return new AuthResultDTO() { Message = "not valid token" };
            }
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive)
            {
                return new AuthResultDTO() { Message = "not active" };
            }
            refreshToken.RevokedOn = DateTime.UtcNow;
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return new AuthResultDTO { Message = ex.Message };
            }
            var newToken = await CreateToken(user);
            return new AuthResultDTO()
            {
                IsAuthenticated = true,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpired = newRefreshToken.ExpiredOn,
                Token = new JwtSecurityTokenHandler().WriteToken(newToken)
            };
        }
        public async Task<bool> RevokeRefreshToken(string token)
        {
            var user = _userManager.Users.SingleOrDefault(t => t.RefreshTokens.Any(t => t.Token == token));
            if (user is null)
            {
                return false;
            }
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
            if (!refreshToken.IsActive)
            {
                return false;
            }
            refreshToken.RevokedOn = DateTime.UtcNow;
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return false;
            }
            await _signInManager.SignOutAsync();
            return true;
        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiredOn = DateTime.UtcNow.AddDays(1),
                CreatedOn = DateTime.UtcNow
            };
        }
        private async Task<JwtSecurityToken> CreateToken(ApplicationUser user, bool resetPassword = false)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name??""),
                new Claim(JwtRegisteredClaimNames.Email,user.Email??""),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            if (resetPassword == true)
            {
                claims.Add(new Claim("Purpose", "ResetPassword"));
            }
            else
            {
                var Roles = await _userManager.GetRolesAsync(user);
                foreach (var Role in Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, Role));
                }
            }
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken myToken = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudiance,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials);
            return myToken;
        }
    }
}
