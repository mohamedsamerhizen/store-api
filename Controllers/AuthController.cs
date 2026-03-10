using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using store.Common;
using store.Dtos.Auth;
using store.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return BadRequest(ApiResponse.FailResponse("Email is already registered."));
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();

            return BadRequest(ApiResponse.FailResponse("Registration failed.", errors));
        }

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Ok(ApiResponse.SuccessResponse("Registered successfully."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized(ApiResponse.FailResponse("Invalid credentials."));
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized(ApiResponse.FailResponse("Invalid credentials."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        var responseData = new
        {
            token,
            email = user.Email,
            fullName = user.FullName,
            roles
        };

        return Ok(ApiResponse.SuccessResponse("Login successful.", responseData));
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var jwt = _config.GetSection("Jwt");

        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var expiryMinutesValue = jwt["ExpiryMinutes"] ?? throw new InvalidOperationException("Jwt:ExpiryMinutes is missing.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("fullName", user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(expiryMinutesValue));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}