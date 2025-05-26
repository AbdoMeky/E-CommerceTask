using Core.DTO;
using Core.DTO.AccountingDTO;
using Core.Interfaces;
using E_CommerceTask.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingRepository _accountingRepository;
        public AccountingController(IAccountingRepository accountingRepository)
        {
            _accountingRepository = accountingRepository;
        }
        [HttpPost("Registe")]
        public async Task<ActionResult> Registe(RegisterDTO user)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() });
            var result = await _accountingRepository.RegisteUser(user);
            if (result.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpired);
                }
                return Ok(new Generalresponse { IsSuccess = true, Data = result });
            }
            return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
        }
        [HttpPost("LogIn")]
        public async Task<ActionResult> LogIn(LogInDTO logInDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() });
            var result = await _accountingRepository.LogIn(logInDTO);
            if (result.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpired);
                }
                return Ok(new Generalresponse { IsSuccess = true, Data = result });
            }
            return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
        }
        [HttpGet("RevokeAndInvoke")]
        public async Task<ActionResult> RevokeAndInvoke()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _accountingRepository.CheckRefreshTokenAndRevoke(refreshToken);
            if (result.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpired);
                }
                return Ok(new Generalresponse { IsSuccess = true, Data = result });
            }
            return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
        }
        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _accountingRepository.RevokeRefreshToken(refreshToken);
            if (result)
            {
                return Ok(new Generalresponse { IsSuccess = true });
            }
            return BadRequest(new Generalresponse { IsSuccess = true, Data = "no valid refresh token to revoke" });
        }
        void SetRefreshTokenInCookie(string refreshToken, DateTime? expiresIn)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresIn?.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
