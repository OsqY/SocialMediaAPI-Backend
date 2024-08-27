using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly SocialMediaDbContext _context;
    private readonly UserManager<ApiUser> _userManager;
    private readonly IConfiguration _configuration;

    public UserController(
        SocialMediaDbContext context,
        UserManager<ApiUser> userManager,
        IConfiguration configuration
    )
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    [ProducesResponseType(typeof(string), 201)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult> RegisterUser(UserDTO userDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ApiUser newUser = new ApiUser();
                newUser.UserName = userDTO.Username;
                newUser.Email = userDTO.Email;
                var result = await _userManager.CreateAsync(newUser, userDTO.Password);

                if (result.Succeeded)
                {
                    return StatusCode(201, $"User {newUser.UserName} created.");
                }
                else
                {
                    throw new Exception(
                        string.Format(
                            "Error: {0}",
                            string.Join(" ", result.Errors.Select(e => e.Description))
                        )
                    );
                }
            }
            else
            {
                var details = new ValidationProblemDetails(ModelState);
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;
                return new BadRequestObjectResult(details);
            }
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();
            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status = StatusCodes.Status500InternalServerError;
            exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
        }
    }

    [HttpPost(Name = "/Login")]
    [ResponseCache(CacheProfileName = "NoCache")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(BadRequestObjectResult), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> Login(LoginDTO loginDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginDTO.Username);

                if (user == null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
                    throw new Exception("Invalid login attempt.");
                else
                {
                    var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!)
                        ),
                        SecurityAlgorithms.HmacSha256
                    );

                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
                    claims.AddRange(
                        (await _userManager.GetRolesAsync(user)).Select(r => new Claim(
                            ClaimTypes.Role,
                            r
                        ))
                    );

                    var jwtObject = new JwtSecurityToken(
                        issuer: _configuration["JWT:Issuer"],
                        audience: _configuration["JWT:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddSeconds(300),
                        signingCredentials: signingCredentials
                    );

                    var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);
                    return StatusCode(StatusCodes.Status200OK, jwtString);
                }
            }
            else
            {
                var details = new ValidationProblemDetails(ModelState);
                details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;

                return new BadRequestObjectResult(details);
            }
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails();

            exceptionDetails.Detail = e.Message;
            exceptionDetails.Status = StatusCodes.Status401Unauthorized;

            exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
        }
    }
}
