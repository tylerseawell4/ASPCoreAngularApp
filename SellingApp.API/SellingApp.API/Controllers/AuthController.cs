using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SellingApp.API.DTOs;
using SellingApp.API.Interfaces;
using SellingApp.API.Models;

namespace SellingApp.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepo authRepo;
        private readonly IConfiguration config;
        public AuthController(IAuthRepo authRepo, IConfiguration config)
        {
            this.authRepo = authRepo;
            this.config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTO userDTO)
        {
            userDTO.Username = userDTO.Username.ToLower();
            if (await authRepo.UserExists(userDTO.Username))
            {
                return BadRequest("Username already taken.");
            }

            var userToCreate = new User
            {
                Username = userDTO.Username,
            };

            var createdUser = await authRepo.Register(userToCreate, userDTO.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userDTO)
        {
            var userFromRepo = await authRepo.Login(userDTO.Username.ToLower(), userDTO.Password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}