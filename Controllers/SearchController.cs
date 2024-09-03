using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Data;
using SocialMediaAPI.DTO;
using SocialMediaAPI.Utils;

namespace SocialMediaAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly SocialMediaDbContext _context;
    private readonly SearchUtils _searchUtils;
    private readonly UserUtils _userUtils;

    public SearchController(
        SocialMediaDbContext context,
        SearchUtils searchUtils,
        UserUtils userUtils
    )
    {
        _context = context;
        _searchUtils = searchUtils;
        _userUtils = userUtils;
    }

    [HttpGet("{search}")]
    public async Task<ActionResult> Search(string search, TYPE_OF_RESULT type)
    {
        var result = await _searchUtils.SearchFromDatabase(
            _context,
            search,
            this,
            _userUtils,
            type
        );

        if (result.Result != null)
            return result.Result;

        return Ok(result.Value);
    }
}
