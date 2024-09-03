using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Models;
using SocialMediaAPI.Utils;

namespace SocialMediaAPI.Controllers;

[Authorize]
[Route("/api/[controller]/[action]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly SocialMediaDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly UserUtils _userUtils;

    public PostController(
        SocialMediaDbContext context,
        IMemoryCache memoryCache,
        UserUtils userUtils
    )
    {
        _context = context;
        _memoryCache = memoryCache;
        _userUtils = userUtils;
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
        var result = await _userUtils.GetUser(_context, User, this);

        if (result.Result != null)
            return result.Result;

        var user = result.Value;

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
            new RestDTO<PostDTO?>()
            {
                Data = new PostDTO
                {
                    Description = post.Description,
                    CreatedDate = post.CreatedDate,
                    Username = user.UserName
                },
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(Url.Action(null, "Posts", postDTO, Request.Scheme)!, "self", "POST")
                }
            }
        );
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _userUtils.GetUser(_context, User, this);

        if (result.Result != null)
            return result.Result;

        var user = result.Value;

        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

        if (post == null)
            return NotFound(
                new RestDTO<string?>()
                {
                    Data = null,
                    Links = new List<LinkDTO>()
                    {
                        new LinkDTO(
                            Url.Action("GetPostFromId", "Posts", new { id }, Request.Scheme)!,
                            "post",
                            "GET"
                        )
                    }
                }
            );

        _context.Remove(post);
        await _context.SaveChangesAsync();

        return Ok(
            new RestDTO<string?>()
            {
                Data = "Post deleted!",
                Links = new List<LinkDTO>()
                {
                    new LinkDTO(
                        Url.Action("DeletePost", "Posts", new { id }, Request.Scheme)!,
                        "post",
                        "DELETE"
                    )
                }
            }
        );
    }

    [HttpPost("{id}")]
    public async Task<ActionResult> LikePost(int id)
    {
        var result = await _userUtils.GetUser(_context, User, this);

        if (result.Result != null)
            return result.Result;

        var user = result.Value;

        var post = await _context
            .Posts.Include(p => p.LikedByUsers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound(new RestDTO<string?> { Data = "Post not found" });

        if (post.LikedByUsers.Any(u => u.Id == user.Id))
            return BadRequest(new RestDTO<string?> { Data = "Post already liked by user." });

        post.LikedByUsers.Add(user);

        return Ok(new RestDTO<string?> { Data = "Post liked." });
    }

    [HttpPost("{id}")]
    public async Task<ActionResult> UnlikePost(int id)
    {
        var result = await _userUtils.GetUser(_context, User, this);

        if (result.Result != null)
            return result.Result;

        var user = result.Value;

        var post = await _context
            .Posts.Include(p => p.LikedByUsers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound(new RestDTO<string?> { Data = "Post not found." });

        if (post.LikedByUsers.Any(u => u.Id == user.Id))
            return BadRequest(new RestDTO<string?> { Data = "Post doesn't have user's like." });

        post.LikedByUsers.Remove(user);

        return Ok(new RestDTO<string?> { Data = "Post unliked." });
    }
}
