using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Utils;

public class UserUtils
{
    public async Task<ActionResult<ApiUser?>> GetUser(
        SocialMediaDbContext context,
        ClaimsPrincipal claims,
        ControllerBase controller
    )
    {
        var username = claims
            .FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            ?.Value;

        if (string.IsNullOrEmpty(username))
            return controller.Unauthorized(new { Message = "User is not authenticated!" });

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            return controller.NotFound(
                new RestDTO<string?>()
                {
                    Data = null,
                    Links = new List<LinkDTO>()
                    {
                        new LinkDTO(
                            controller.Url.Action(
                                "GetUsers",
                                "Users",
                                null,
                                controller.Request.Scheme
                            )!,
                            "users",
                            "GET"
                        )
                    }
                }
            );
        }
        return user;
    }
}
