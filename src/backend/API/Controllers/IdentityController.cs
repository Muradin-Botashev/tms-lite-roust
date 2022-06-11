using Domain.Extensions;
using Domain.Services.Identity;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace API.Controllers
{
    [Route("api/identity")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        /// <summary>
        /// Получение информации о пользователе
        /// </summary>
        [HttpGet("userInfo")]
        [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
        public IActionResult UserInfo()
        {
            try
            {
                UserInfo result = identityService.GetUserInfo();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Get user info");
                return StatusCode(500, ex.Message);
            }
        }
        
        /// <summary>
        /// Авторизация, получение токена для логина и пароля
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody]IdentityModel model)
        {
            try
            {
                var identity = identityService.GetToken(model);

                switch (identity.Result)
                {
                    case VerificationResult.Ok:
                        return Ok(identity.Data);

                    case VerificationResult.Forbidden:
                        return BadRequest("UserNotFound");

                    case VerificationResult.WrongCredentials:
                        return BadRequest("UserIncorrectData");

                    default:
                        return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Login");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Смена мастер-пароля
        /// </summary>
        [HttpPost("masterpassword")]
        [Authorize(AuthenticationSchemes = ApiExtensions.BasicApiSchemes, Policy = ApiExtensions.BasicApiPolicy)]
        public IActionResult ChangeMasterPassword([FromBody]Domain.Services.Profile.SetNewPasswordDto model)
        {
            try
            {
                var result = identityService.ChangeMasterPassword(model.OldPassword, model.NewPassword);
                if (result.IsError)
                {
                    return BadRequest(result);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Change master password");
                return StatusCode(500, ex.Message);
            }
        }
    }
}