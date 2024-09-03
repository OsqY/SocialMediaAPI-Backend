using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;

namespace SocialMediaAPI.Utils;

public class SearchUtils
{
    public async Task<ActionResult<SearchResultDTO?>> SearchFromDatabase(
        SocialMediaDbContext context,
        string search,
        ControllerBase controller,
        UserUtils userUtils,
        TYPE_OF_RESULT searchType
    )
    {
        switch (searchType)
        {
            case TYPE_OF_RESULT.USERS:
                var result = await userUtils.GetUserProfileInfo(context, search, controller);

                if (result.Result != null)
                    return result.Result;

                return new SearchResultDTO
                {
                    Username = result.Value.Username,
                    ResultType = TYPE_OF_RESULT.USERS
                };
            case TYPE_OF_RESULT.ALL:
                break;
            case TYPE_OF_RESULT.POSTS:
                var posts = await context
                    .Posts.Where(p => p.Description.Contains(search))
                    .Select(p => new PostDTO
                    {
                        Description = p.Description,
                        CreatedDate = p.CreatedDate,
                        Username = p.User.UserName
                    })
                    .ToListAsync();

                return new SearchResultDTO { Posts = posts, ResultType = TYPE_OF_RESULT.POSTS };
        }
        return controller.BadRequest(new { Message = "Invalid search type" });
    }
}
