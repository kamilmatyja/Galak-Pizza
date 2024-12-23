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
public class EntryController : Controller
{
    private readonly ApplicationDbContext _context;

    public EntryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Entry
    public async Task<IActionResult> Index(int? pageId, int? userId)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        var query = _context.EntryModel
            .Include(e => e.Page)
            .Include(e => e.User)
            .ThenInclude(u => u.IdentityUser)
            .AsQueryable();

        if (pageId.HasValue)
        {
            query = query.Where(e => e.PageId == pageId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(e => e.UserId == userId.Value);
        }

        ViewData["FilterPage"] = new SelectList(await _context.PageModel.ToListAsync(), "Id", "Title", pageId);

        var users = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToListAsync();
        ViewData["FilterUser"] = new SelectList(users, "Id", "UserName", userId);

        return View(await query.ToListAsync());
    }

    // GET: Entry/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        if (id == null) return NotFound();

        var entryModel = await _context.EntryModel
            .Include(e => e.Page)
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entryModel == null) return NotFound();

        return View(entryModel);
    }

    // GET: Entry/Create
    public IActionResult Create()
    {
        if (!UserHasRole()) return Unauthorized();

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title");

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", GetUserId());

        var entryModel = new EntryModel();
        entryModel.CreatedAt = DateTime.Now;

        return View(entryModel);
    }

    // POST: Entry/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PageId,UserId,CreatedAt")] EntryModel entryModel)
    {
        if (!UserHasRole()) return Unauthorized();

        if (!await _context.PageModel.AnyAsync(p => p.Id == entryModel.PageId))
            ModelState.AddModelError("PageId", "Błędna podstrona.");
        else
            entryModel.Page = await _context.PageModel.FindAsync(entryModel.PageId);

        if (entryModel.UserId != null)
        {
            if (!await _context.UserModel.AnyAsync(u => u.Id == entryModel.UserId))
                ModelState.AddModelError("UserId", "Błędny użytkownik.");
            else
                entryModel.User = await _context.UserModel.FindAsync(entryModel.UserId);
        }

        if (ModelState.IsValid)
        {
            _context.Add(entryModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title", entryModel.PageId);

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", entryModel.UserId);

        return View(entryModel);
    }

    // GET: Entry/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var entryModel = await _context.EntryModel.FindAsync(id);
        if (entryModel == null) return NotFound();

        ViewData["PageId"] = new SelectList(_context.PageModel, "Id", "Title", entryModel.PageId);

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", entryModel.UserId);

        return View(entryModel);
    }

    // POST: Entry/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PageId,UserId,CreatedAt")] EntryModel entryModel)
    {
        if (!UserHasRole()) return Unauthorized();

        var existingModel = await _context.EntryModel.FirstOrDefaultAsync(m => m.Id == id);
        if (existingModel == null) return NotFound();

        if (!await _context.PageModel.AnyAsync(p => p.Id == entryModel.PageId))
        {
            ModelState.AddModelError("PageId", "Błędna podstrona.");
        }
        else
        {
            existingModel.PageId = entryModel.PageId;
            existingModel.Page = await _context.PageModel.FindAsync(entryModel.PageId);
        }

        if (entryModel.UserId != null)
        {
            if (!await _context.UserModel.AnyAsync(u => u.Id == entryModel.UserId))
            {
                ModelState.AddModelError("UserId", "Błędny użytkownik.");
            }
            else
            {
                existingModel.UserId = entryModel.UserId;
                existingModel.User = await _context.UserModel.FindAsync(entryModel.UserId);
            }
        }

        existingModel.CreatedAt = entryModel.CreatedAt;

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

        return View(existingModel);
    }

    // GET: Entry/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var entryModel = await _context.EntryModel
            .Include(e => e.Page)
            .Include(e => e.User)
            .ThenInclude(u => u.IdentityUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entryModel == null) return NotFound();

        return View(entryModel);
    }

    // POST: Entry/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!UserHasRole()) return Unauthorized();

        var entryModel = await _context.EntryModel.FindAsync(id);
        if (entryModel != null) _context.EntryModel.Remove(entryModel);

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