using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Enums;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Controllers;

public class PageController : Controller
{
    private readonly ApplicationDbContext _context;

    public PageController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("/cms/{*link}")]
    public async Task<IActionResult> Home(string? link)
    {
        var pageModel = await _context.PageModel
            .Include(p => p.Category)
            .Include(p => p.ParentPage)
            .Include(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(u => u.Entries)
            .Include(u => u.Comments)
            .ThenInclude(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(u => u.Ratings)
            .ThenInclude(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(u => u.Contents)
            .FirstOrDefaultAsync(m => m.Link == link);

        if (pageModel == null)
        {
            if (link != null)
            {
                return NotFound();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        var entry = new EntryModel
        {
            PageId = pageModel.Id,
            Page = pageModel,
            CreatedAt = DateTime.Now
        };

        var userId = GetUserId();

        if (userId != 0)
        {
            entry.UserId = userId;
            entry.User = await _context.UserModel.FindAsync(userId);
        }

        _context.EntryModel.Add(entry);
        await _context.SaveChangesAsync();

        ViewData["ChildrenPages"] = await _context.PageModel
            .Where(p => p.ParentPageId == pageModel.Id)
            .ToListAsync();

        ViewData["Rating"] = EnumExtensions.ToSelectList<RatingsEnum>();

        return View(pageModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRating(int pageId, RatingsEnum rating)
    {
        var page = await _context.PageModel.FirstOrDefaultAsync(m => m.Id == pageId);
        if (page == null) return NotFound();

        var userId = GetUserId();

        if (await _context.RateModel.AnyAsync(r =>
                r.UserId == userId && r.PageId == page.Id))
        {
            return RedirectToAction("Home", new { link = page.Link });
        }

        var rate = new RateModel
        {
            PageId = pageId,
            Page = page,
            UserId = userId,
            User = await _context.UserModel.FindAsync(userId),
            CreatedAt = DateTime.Now,
            Rating = rating,
            Status = InteractionStatusesEnum.Unverified
        };

        _context.RateModel.Add(rate);
        await _context.SaveChangesAsync();

        return RedirectToAction("Home", new { link = page.Link });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRating(int pageId)
    {
        var page = await _context.PageModel.FirstOrDefaultAsync(m => m.Id == pageId);
        if (page == null) return NotFound();

        var userId = GetUserId();

        var rate = await _context.RateModel
            .FirstOrDefaultAsync(r => r.PageId == pageId && r.UserId == userId);
        if (rate == null) return NotFound();

        _context.RateModel.Remove(rate);
        await _context.SaveChangesAsync();

        return RedirectToAction("Home", new { link = page.Link });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PushComment(int pageId, int? commentId, string description)
    {
        var page = await _context.PageModel.FirstOrDefaultAsync(m => m.Id == pageId);
        if (page == null) return NotFound();

        var userId = GetUserId();

        CommentModel comment;

        if (commentId.HasValue)
        {
            comment = await _context.CommentModel.FirstOrDefaultAsync(c => c.Id == commentId.Value && c.PageId == pageId);
            if (comment == null) return NotFound();

            if (comment.UserId != userId) return Unauthorized();

            comment.Description = description;
            comment.Status = InteractionStatusesEnum.Unverified;
            _context.CommentModel.Update(comment);
        }
        else
        {
            comment = new CommentModel
            {
                PageId = pageId,
                Page = page,
                UserId = userId,
                User = await _context.UserModel.FindAsync(userId),
                CreatedAt = DateTime.Now,
                Description = description,
                Status = InteractionStatusesEnum.Unverified
            };

            _context.CommentModel.Add(comment);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Home", new { link = page.Link });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int pageId, int commentId)
    {
        var page = await _context.PageModel.FirstOrDefaultAsync(m => m.Id == pageId);
        if (page == null) return NotFound();

        var comment = await _context.CommentModel.FirstOrDefaultAsync(c => c.Id == commentId && c.PageId == pageId);
        if (comment == null) return NotFound();

        var userId = GetUserId();
        if (comment.UserId != userId) return Unauthorized();

        _context.CommentModel.Remove(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Home", new { link = page.Link });
    }

    // GET: Page
    public async Task<IActionResult> Index(
        int? userId,
        int? parentPageId,
        int? categoryId,
        string? title)
    {
        var query = _context.PageModel
            .Include(p => p.Category)
            .Include(p => p.ParentPage)
            .Include(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(p => p.Entries)
            .Include(p => p.Comments)
            .Include(p => p.Ratings)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (parentPageId.HasValue)
        {
            query = query.Where(p => p.ParentPageId == parentPageId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(p => EF.Functions.Like(p.Title, $"%{title}%"));
        }

        var users = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToListAsync();
        ViewData["FilterUser"] = new SelectList(users, "Id", "UserName", userId);

        var parentPages = await _context.PageModel.ToListAsync();
        ViewData["FilterParentPageId"] = new SelectList(parentPages, "Id", "Title", parentPageId);

        var categories = await _context.CategoryModel.ToListAsync();
        ViewData["FilterCategory"] = new SelectList(categories, "Id", "Name", categoryId);

        ViewData["FilterTitle"] = title;

        return View(await query.ToListAsync());
    }

    // GET: Page/Details/5
    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        if (id == null) return NotFound();

        var pageModel = await _context.PageModel
            .Include(p => p.Category)
            .Include(p => p.ParentPage)
            .Include(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(u => u.Entries)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .Include(u => u.Contents)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (pageModel == null) return NotFound();

        return View(pageModel);
    }

    // GET: Page/Create
    [Authorize]
    public IActionResult Create()
    {
        if (!UserHasRole()) return Unauthorized();

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", GetUserId());

        ViewData["ParentPageId"] = new SelectList(_context.PageModel, "Id", "Title");

        ViewData["CategoryId"] = new SelectList(_context.CategoryModel, "Id", "Name");

        var pageModel = new PageModel();
        pageModel.CreatedAt = DateTime.Now;

        return View(pageModel);
    }

    // POST: Page/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("UserId,ParentPageId,CategoryId,CreatedAt,Link,Title,Description,Keywords,Image")] PageModel pageModel,
        List<string?> contentValues, List<string> contentTypes)
    {
        if (!UserHasRole()) return Unauthorized();

        if (!await _context.UserModel.AnyAsync(u => u.Id == pageModel.UserId))
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        else
            pageModel.User = await _context.UserModel.FindAsync(pageModel.UserId);

        if (pageModel.ParentPageId != null)
        {
            if (!await _context.PageModel.AnyAsync(p => p.Id == pageModel.ParentPageId))
                ModelState.AddModelError("ParentPageId", "Błędna podstrona.");
            else if (pageModel.ParentPageId == pageModel.Id)
                ModelState.AddModelError("ParentPageId", "Nie można ustawić strony nadrzędnej jako siebie samego.");
            else
                pageModel.ParentPage = await _context.PageModel.FindAsync(pageModel.ParentPageId);
        }

        if (!await _context.CategoryModel.AnyAsync(p => p.Id == pageModel.CategoryId))
            ModelState.AddModelError("CategoryId", "Błędna kategoria.");
        else
            pageModel.Category = await _context.CategoryModel.FindAsync(pageModel.CategoryId);

        if (await _context.PageModel.AnyAsync(p => p.Link == pageModel.Link))
            ModelState.AddModelError("Link", "Strona z tym linkiem już istnieje.");

        ValidateImageInput(pageModel.Image, pageModel);

        var contents = ProcessPageContents(contentValues, contentTypes, pageModel);

        if (ModelState.IsValid)
        {
            _context.Add(pageModel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", pageModel.UserId);

        ViewData["ParentPageId"] = new SelectList(_context.PageModel, "Id", "Title", pageModel.ParentPageId);

        ViewData["CategoryId"] = new SelectList(_context.CategoryModel, "Id", "Name", pageModel.CategoryId);

        ViewData["Contents"] = contents;

        return View(pageModel);
    }

    // GET: Page/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var pageModel = await _context.PageModel
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pageModel == null) return NotFound();

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", pageModel.UserId);

        ViewData["ParentPageId"] = new SelectList(
            _context.PageModel.Where(p => p.Id != pageModel.Id),
            "Id",
            "Title",
            pageModel.ParentPageId
        );

        ViewData["CategoryId"] = new SelectList(_context.CategoryModel, "Id", "Name", pageModel.CategoryId);

        return View(pageModel);
    }

    // POST: Page/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,UserId,ParentPageId,CategoryId,CreatedAt,Link,Title,Description,Keywords,Image")] PageModel pageModel,
        List<string?> contentValues, List<string> contentTypes)
    {
        if (!UserHasRole()) return Unauthorized();

        var existingModel = await _context.PageModel.FirstOrDefaultAsync(m => m.Id == id);
        if (existingModel == null) return NotFound();

        if (!await _context.UserModel.AnyAsync(u => u.Id == existingModel.UserId))
        {
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        }
        else
        {
            existingModel.UserId = pageModel.UserId;
            existingModel.User = await _context.UserModel.FindAsync(pageModel.UserId);
        }

        if (pageModel.ParentPageId != null)
        {
            if (!await _context.PageModel.AnyAsync(p => p.Id == pageModel.ParentPageId))
            {
                ModelState.AddModelError("ParentPageId", "Błędna podstrona.");
            }
            else if (pageModel.ParentPageId == id)
            {
                ModelState.AddModelError("ParentPageId", "Nie można ustawić strony nadrzędnej jako siebie samego.");
            }
            else
            {
                existingModel.ParentPageId = pageModel.ParentPageId;
                existingModel.ParentPage = await _context.PageModel.FindAsync(pageModel.ParentPageId);
            }
        }
        else
        {
            existingModel.ParentPageId = null;
            existingModel.ParentPage = null;
        }

        if (!await _context.CategoryModel.AnyAsync(p => p.Id == pageModel.CategoryId))
        {
            ModelState.AddModelError("CategoryId", "Błędna kategoria.");
        }
        else
        {
            existingModel.CategoryId = pageModel.CategoryId;
            existingModel.Category = await _context.CategoryModel.FindAsync(pageModel.CategoryId);
        }

        if (await _context.PageModel.AnyAsync(p => p.Link == pageModel.Link && p.Id != id))
            ModelState.AddModelError("Link", "Strona z tym linkiem już istnieje.");

        existingModel.CreatedAt = pageModel.CreatedAt;
        existingModel.Link = pageModel.Link;
        existingModel.Title = pageModel.Title;
        existingModel.Description = pageModel.Description;
        existingModel.Keywords = pageModel.Keywords;
        existingModel.Image = pageModel.Image;

        ValidateImageInput(existingModel.Image, existingModel);

        _context.PageContentModels.RemoveRange(_context.PageContentModels.Where(pc => pc.PageId == existingModel.Id));
        existingModel.Contents.Clear();

        var contents = ProcessPageContents(contentValues, contentTypes, existingModel);

        if (ModelState.IsValid)
        {
            _context.Update(existingModel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", existingModel.UserId);

        ViewData["ParentPageId"] = new SelectList(
            _context.PageModel.Where(p => p.Id != existingModel.Id),
            "Id",
            "Title",
            existingModel.ParentPageId
        );

        ViewData["CategoryId"] = new SelectList(_context.CategoryModel, "Id", "Name", existingModel.CategoryId);

        ViewData["Contents"] = contents;

        return View(existingModel);
    }

    // GET: Page/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var pageModel = await _context.PageModel
            .Include(p => p.Category)
            .Include(p => p.ParentPage)
            .Include(p => p.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(u => u.Entries)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .Include(u => u.Contents)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (pageModel == null) return NotFound();

        return View(pageModel);
    }

    // POST: Page/Delete/5
    [Authorize]
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!UserHasRole()) return Unauthorized();

        var pageModel = await _context.PageModel
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pageModel != null) _context.PageModel.Remove(pageModel);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private int GetUserId()
    {
        var IdentifierUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return _context.UserModel
            .Where(u => u.IdentityUser.Id == IdentifierUserId)
            .Select(u => u.Id)
            .FirstOrDefault();
    }

    private bool UserHasRole(UserRolesEnum? roleToCheck = null)
    {
        var identifierUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return _context.UserModel
            .Any(u => u.IdentityUser.Id == identifierUserId &&
                      (u.Role == UserRolesEnum.Author ||
                       u.Role == UserRolesEnum.Administrator ||
                       (roleToCheck.HasValue && u.Role == roleToCheck.Value)));
    }

    private bool IsValidBase64Image(string imageValue)
    {
        if (string.IsNullOrEmpty(imageValue)) return false;

        try
        {
            var imageBytes = Convert.FromBase64String(imageValue);

            if (imageBytes.Length > 4)
            {
                // JPEG (FF D8 FF)
                if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF) return true;

                // PNG (89 50 4E 47)
                if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E &&
                    imageBytes[3] == 0x47) return true;

                // GIF (47 49 46 38)
                if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 &&
                    imageBytes[3] == 0x38) return true;

                // WEBP (52 49 46 46 - RIFF Header)
                if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 &&
                    imageBytes[3] == 0x46) return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private void ValidateImageInput(string? imageValue, PageModel pageModel)
    {
        if (string.IsNullOrEmpty(imageValue))
        {
            ModelState.AddModelError("Image", "To pole jest wymagane.");
        }
        else
        {
            if (!IsValidBase64Image(imageValue))
                ModelState.AddModelError("Image", "Niepoprawny format.");
            else
                pageModel.Image = imageValue;
        }
    }

    private List<string> ProcessPageContents(List<string?> contentValues, List<string> contentTypes, PageModel pageModel)
    {
        List<string> contents = new();

        for (var i = 0; i < contentTypes.Count; i++)
        {
            var value = contentValues[i];

            var pageContent = new PageContentModel
            {
                PageId = pageModel.Id,
                Page = pageModel,
                Order = i + 1
            };

            if (contentTypes[i] == ContentTypesEnum.Image.ToString())
            {
                pageContent.Type = ContentTypesEnum.Image;

                if (string.IsNullOrEmpty(value))
                {
                    contents.Add("To pole jest wymagane.");
                    ModelState.AddModelError("Contents", "To pole jest wymagane.");
                }
                else
                {
                    if (!IsValidBase64Image(value))
                    {
                        contents.Add("Niepoprawny format.");
                        ModelState.AddModelError("Contents", "Niepoprawny format.");
                    }
                    else
                    {
                        contents.Add("");
                        pageContent.Value = value;
                    }
                }
            }
            else
            {
                pageContent.Type = ContentTypesEnum.Text;

                if (value == null)
                {
                    contents.Add("To pole jest wymagane.");
                    ModelState.AddModelError("Contents", "To pole jest wymagane.");
                }
                else
                {
                    contents.Add("");
                    pageContent.Value = value;
                }
            }

            _context.Add(pageContent);
        }

        return contents;
    }
}