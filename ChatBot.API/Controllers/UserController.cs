﻿using ChatBot.Core.Dtos;
using ChatBot.Core.Interfaces;
using ChatBot.Core.Models;
using ChatBot.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChatBot.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILoggerManager _logger;

        public UserController(UserManager<User> userManager, ILoggerManager logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users;

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogError($"User with id = {id} not found");
                return NotFound();
            }
            return Ok(user);
        }

        [Authorize(Policy = PolicyTypes.Users.Manage)]
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserForCreationDto userDto)
        {
            if (userDto == null)
            {
                _logger.LogWarn("UserDto can't be null");
                return BadRequest();
            }

            var user = new User()
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            if (userDto.Roles != null && userDto.Roles.Count != 0)
            {
                await _userManager.AddToRolesAsync(user, userDto.Roles);
            }
            return StatusCode(201);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UserForUpdateDto userDto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogError($"User with id = {id} not found");
                return NotFound();
            }
            user.Email = userDto.Email;
            user.UserName = userDto.UserName;

            await _userManager.UpdateAsync(user);
            return Ok(user);
        }

        [HttpPut("{id}/password-change")]
        public async Task<IActionResult> ChangeUsersPassword(string id, PasswordChangeDto passwordsDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogError($"User with id = {id} not found");
                return NotFound();
            }
            if (!await _userManager.CheckPasswordAsync(user, passwordsDto.OldPassword))
            {
                _logger.LogError("Wrong old password");
                return BadRequest("Wrong old password");
            }
            var res = await _userManager.ChangePasswordAsync(user, passwordsDto.OldPassword, passwordsDto.NewPassword);
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.LogError($"User with id = {id} not found");
                return NotFound();
            }

            await _userManager.DeleteAsync(user);
            return Ok(user);
        }

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRolesForUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogError($"User with id = {id} not found");
                return NotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            return Ok(userRoles);
        }
    }
}
