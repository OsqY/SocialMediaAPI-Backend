using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;

namespace SocialMediaAPI.Controllers;

[Authorize]
[Route("/api/[controller]/[action]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly SocialMediaDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public PostController(SocialMediaDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    [HttpGet("{username}")]
    public async Task<RestDTO<Post[]>> GetPostsFromUser(string username)
    {
        (int recordCount, Post[]? result) dataTuple = (0, null);

        var cacheKey = $"GetPostsFrom{username}";

        if (!_memoryCache.TryGetValue(cacheKey, out dataTuple))
        {
            Console.WriteLine("nothing in cache");

            var query = _context
                .Posts.AsQueryable()
                .Where(p => p.User!.UserName == username)
                .OrderBy(p => p.CreatedDate);
            dataTuple.recordCount = await query.CountAsync();

            dataTuple.result = await query.ToArrayAsync();
            _memoryCache.Set(cacheKey, dataTuple, new TimeSpan(0, 0, 15));
        }

        return new RestDTO<Post[]>()
        {
            Data = dataTuple.result,
            RecordCount = dataTuple.recordCount,
            Links = new List<LinkDTO>()
            {
                new LinkDTO(Url.Action(null, "Posts", Request.Scheme)!, "self", "GET"),
            },
        };
    }

    [HttpPost]
    public async Task<ActionResult> Post(PostDTO postDTO)
    {
        var username = User.FindFirst(
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        )?.Value;
        Console.WriteLine("Username" + username);
        Console.WriteLine(User.Claims);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            return NotFound(
                new RestDTO<Post?>()
                {
                    Data = null,
                    Links = new List<LinkDTO>()
                    {
                        new LinkDTO(
                            Url.Action("GetUsers", "Users", null, Request.Scheme)!,
                            "users",
                            "GET"
                        )
                    }
                }
            );
        }

        Post? post = new Post
        {
            Description = postDTO.Description,
            UserId = user.Id,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now,
        };

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        return Ok(
            new RestDTO<Post?>()
            {
                Data = post,
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(Url.Action(null, "Posts", postDTO, Request.Scheme)!, "self", "POST")
                }
            }
        );
    }
}
