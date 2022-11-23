using API.DTOs;
using API.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly TokenService tokenService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            AppUser user = await userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized();
            }

            Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                return await CreateUserObject(user);

            }

            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await userManager.Users.AnyAsync(_ => _.Email == registerDto.Email))
            {
                return BadRequest("Email taken");
            }
            if (await userManager.Users.AnyAsync(_ => _.UserName == registerDto.UserName))
            {
                return BadRequest("Username taken");

            }

            AppUser user = new AppUser()
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                return await CreateUserObject(user);

            }

            return BadRequest("Problem registering user");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser(RegisterDto registerDto)
        {
            var user = await userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));

            return await CreateUserObject(user);
        }



        [HttpPost("refreshToken")]
        [Authorize]
        public async Task<ActionResult<UserDto>> RefreshToken()
        {
            var refreshTToken = Request.Cookies["refreshToken"];

            var user = await userManager.Users.Include(_ => _.RefreshTokens)
                .FirstOrDefaultAsync(_ => _.UserName == User.FindFirstValue(ClaimTypes.Name));

            if (user == null)
            {
                return Unauthorized();
            }

            var oldToken = user.RefreshTokens.SingleOrDefault(_ => _.Token == refreshTToken);

            if (oldToken != null && !oldToken.IsActive)
            {
                return Unauthorized();
            }

            //if (oldToken != null) oldToken.Revoked = DateTime.UtcNow;

            return await CreateUserObject(user, false);
        }

        private async Task<UserDto> CreateUserObject(AppUser user, bool refreshToken = true)
        {
            if (refreshToken)
                await SetRefreshToken(user);

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = null,
                Token = tokenService.CreateToken(user),
                UserName = user.UserName
            };
        }

        private async Task SetRefreshToken(AppUser user)
        {
            var refreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);

            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),

            };

            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
    }
}
