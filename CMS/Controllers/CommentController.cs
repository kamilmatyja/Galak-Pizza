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

[Authorize]
public class CommentController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Comment
    public async Task<IActionResult> Index(int? pageId, int? userId, string? description, InteractionStatusesEnum? status)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        var query = _context.CommentModel
            .Include(c => c.Page)
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .AsQueryable();

        if (pageId.HasValue)
        {
            query = query.Where(c => c.PageId == pageId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(description))
        {
            query = query.Where(c => EF.Functions.Like(c.Description, $"%{description}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        ViewData["FilterPage"] = new SelectList(await _context.PageModel.ToListAsync(), "Id", "Title", pageId);

        var users = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToListAsync();
        ViewData["FilterUser"] = new SelectList(users, "Id", "UserName", userId);

        ViewData["FilterStatus"] = EnumExtensions.ToSelectList<InteractionStatusesEnum>(status);

        ViewData["FilterDescription"] = description;

        return View(await query.ToListAsync());
    }

    // GET: Comment/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        if (id == null) return NotFound();

        var commentModel = await _context.CommentModel
            .Include(c => c.Page)
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (commentModel == null) return NotFound();

        return View(commentModel);
    }

    // GET: Comment/Create
    public IActionResult Create()
    {
        if (!UserHasRole()) return Unauthorized();

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title");

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", GetUserId());

        var commentModel = new CommentModel();
        commentModel.CreatedAt = DateTime.Now;

        ViewData["Status"] = EnumExtensions.ToSelectList<InteractionStatusesEnum>();

        return View(commentModel);
    }

    // POST: Comment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("PageId,UserId,CreatedAt,Description,Status")] CommentModel commentModel)
    {
        if (!UserHasRole()) return Unauthorized();

        if (!await _context.PageModel.AnyAsync(p => p.Id == commentModel.PageId))
            ModelState.AddModelError("PageId", "Błędna podstrona.");
        else
            commentModel.Page = await _context.PageModel.FindAsync(commentModel.PageId);

        if (!await _context.UserModel.AnyAsync(u => u.Id == commentModel.UserId))
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        else
            commentModel.User = await _context.UserModel.FindAsync(commentModel.UserId);

        if (ModelState.IsValid)
        {
            _context.Add(commentModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title", commentModel.PageId);

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", commentModel.UserId);

        ViewData["Status"] = EnumExtensions.ToSelectList<InteractionStatusesEnum>(commentModel.Status);

        return View(commentModel);
    }

    // GET: Comment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var commentModel = await _context.CommentModel.FindAsync(id);
        if (commentModel == null) return NotFound();

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title", commentModel.PageId);

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", commentModel.UserId);

        ViewData["Status"] = EnumExtensions.ToSelectList<InteractionStatusesEnum>(commentModel.Status);

        return View(commentModel);
    }

    // POST: Comment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,PageId,UserId,CreatedAt,Description,Status")] CommentModel commentModel)
    {
        if (!UserHasRole()) return Unauthorized();

        var existingModel = await _context.CommentModel.FirstOrDefaultAsync(m => m.Id == id);
        if (existingModel == null) return NotFound();

        if (!await _context.PageModel.AnyAsync(p => p.Id == commentModel.PageId))
        {
            ModelState.AddModelError("PageId", "Błędna podstrona.");
        }
        else
        {
            existingModel.PageId = commentModel.PageId;
            existingModel.Page = await _context.PageModel.FindAsync(commentModel.PageId);
        }

        if (!await _context.UserModel.AnyAsync(u => u.Id == commentModel.UserId))
        {
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        }
        else
        {
            existingModel.UserId = commentModel.UserId;
            existingModel.User = await _context.UserModel.FindAsync(commentModel.UserId);
        }

        existingModel.CreatedAt = commentModel.CreatedAt;
        existingModel.Description = commentModel.Description;
        existingModel.Status = commentModel.Status;

        if (ModelState.IsValid)
        {
            _context.Update(existingModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title", existingModel.PageId);

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", existingModel.UserId);

        ViewData["Status"] = EnumExtensions.ToSelectList<InteractionStatusesEnum>(existingModel.Status);

        return View(existingModel);
    }

    // GET: Comment/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var commentModel = await _context.CommentModel
            .Include(c => c.Page)
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (commentModel == null) return NotFound();

        return View(commentModel);
    }

    // POST: Comment/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!UserHasRole()) return Unauthorized();

        var commentModel = await _context.CommentModel.FindAsync(id);
        if (commentModel != null) _context.CommentModel.Remove(commentModel);

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
                      (u.Role == UserRolesEnum.Moderator ||
                       u.Role == UserRolesEnum.Administrator ||
                       (roleToCheck.HasValue && u.Role == roleToCheck.Value)));
    }
}