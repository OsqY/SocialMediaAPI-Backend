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

        ApiUser user;
        user = await context.Users.FirstOrDefaultAsync(u => u.UserName == username);

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

    public async Task<ActionResult<UserProfileDTO?>> GetUserProfileInfo(
        SocialMediaDbContext context,
        string username,
        ControllerBase controller
    )
    {
        if (string.IsNullOrEmpty(username))
            return controller.NotFound(new { Message = "User not found" });

        var userProfile = await context
            .Users.Where(u => u.UserName == username)
            .Select(u => new UserProfileDTO
            {
                Username = username,
                FollowingCount = u.Following.Count(),
                FollowersCount = u.Followers.Count(),
                Posts = u
                    .Posts.Select(p => new PostDTO
                    {
                        Description = p.Description,
                        CreatedDate = p.CreatedDate
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (userProfile == null)
            return controller.NotFound();

        return userProfile;
    }
}
