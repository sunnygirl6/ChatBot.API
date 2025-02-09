﻿using ChatBot.Core.Dtos;
using ChatBot.Core.Interfaces;
using ChatBot.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ChatBot.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : Controller
    {
        private readonly ILoggerManager _logger;
        private readonly IAuthenticationManager _authManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController( 
                                        ILoggerManager logger,
                                        IAuthenticationManager authenticationManager,
                                        IConfiguration configuration, 
                                        UserManager<User> userManager)
        {
            _logger = logger;
            _authManager = authenticationManager;
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <summary>
        /// Login the system
        /// </summary>
        /// <returns>The token</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> AuthenticateUser(UserForAuthenticationDto userDto)
        {
            if (!await _authManager.ValidateUser(userDto))
            {
                _logger.LogWarn($"Authentication of user failed. {userDto.UserName} {userDto.Password}");
                return Unauthorized();
            }
            var user = await _userManager.FindByNameAsync(userDto.UserName);
            var response = new
            {
                token = await _authManager.CreateToken(),
                minutesExpires = _configuration.GetSection("JwtSettings").GetSection("minutesExpires").Value,
                roles = await _userManager.GetRolesAsync(user)
            };
            return Ok(response);
        }
    }
}
