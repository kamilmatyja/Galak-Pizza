using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Enums;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: User
    public async Task<IActionResult> Index(UserRolesEnum? role)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        var query = _context.UserModel
            .Include(u => u.IdentityUser)
            .Include(u => u.Pages)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .Include(u => u.Entries)
            .AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        ViewData["FilterRole"] = EnumExtensions.ToSelectList<UserRolesEnum>(role);

        return View(await query.ToListAsync());
    }

    // GET: User/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        if (id == null) return NotFound();

        var userModel = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Include(u => u.Pages)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .Include(u => u.Entries)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (userModel == null) return NotFound();

        return View(userModel);
    }

    // GET: User/Create
    public IActionResult Create()
    {
        if (!UserHasRole()) return Unauthorized();

        var availableUsers = _context.Users
            .Where(u => !_context.UserModel.Select(um => um.IdentityUserId).Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        ViewData["IdentityUserId"] = new SelectList(availableUsers, "Id", "UserName", GetCurrentUserIdentifierUserId());

        var userModel = new UserModel();
        userModel.CreatedAt = DateTime.Now;

        ViewData["Role"] = EnumExtensions.ToSelectList<UserRolesEnum>();

        return View(userModel);
    }

    // POST: User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdentityUserId,CreatedAt,Role")] UserModel userModel)
    {
        if (!UserHasRole()) return Unauthorized();

        if (!await _context.Users.AnyAsync(p => p.Id == userModel.IdentityUserId))
            ModelState.AddModelError("IdentityUserId", "Błędny użytkownik.");
        else
            userModel.IdentityUser = await _context.Users.FindAsync(userModel.IdentityUserId);

        if (await _context.UserModel.AnyAsync(u => u.IdentityUserId == userModel.IdentityUserId))
            ModelState.AddModelError("IdentityUserId", "Konto z tym użytkownikiem już istnieje.");

        if (ModelState.IsValid)
        {
            _context.Add(userModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var availableUsers = _context.Users
            .Where(u => !_context.UserModel.Select(um => um.IdentityUserId).Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        ViewData["IdentityUserId"] = new SelectList(availableUsers, "Id", "UserName", userModel.IdentityUserId);

        ViewData["Role"] = EnumExtensions.ToSelectList<UserRolesEnum>(userModel.Role);

        return View(userModel);
    }

    // GET: User/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var userModel = await _context.UserModel.FindAsync(id);
        if (userModel == null) return NotFound();

        var availableUsers = _context.Users
            .Where(u => !_context.UserModel.Select(um => um.IdentityUserId).Contains(u.Id) ||
                        u.Id == userModel.IdentityUserId)
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        ViewData["IdentityUserId"] = new SelectList(availableUsers, "Id", "UserName", userModel.IdentityUserId);

        ViewData["Role"] = EnumExtensions.ToSelectList<UserRolesEnum>(userModel.Role);

        return View(userModel);
    }

    // POST: User/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,IdentityUserId,CreatedAt,Role")] UserModel userModel)
    {
        if (!UserHasRole()) return Unauthorized();

        var existingModel = await _context.UserModel.FirstOrDefaultAsync(m => m.Id == id);
        if (existingModel == null) return NotFound();

        if (!await _context.Users.AnyAsync(p => p.Id == userModel.IdentityUserId))
        {
            ModelState.AddModelError("IdentityUserId", "Błędny użytkownik.");
        }
        else
        {
            existingModel.IdentityUserId = userModel.IdentityUserId;
            existingModel.IdentityUser = await _context.Users.FindAsync(userModel.IdentityUserId);
        }

        if (await _context.UserModel.AnyAsync(u => u.IdentityUserId == userModel.IdentityUserId && u.Id != id))
            ModelState.AddModelError("IdentityUserId", "Konto z tym użytkownikiem już istnieje.");

        existingModel.CreatedAt = userModel.CreatedAt;
        existingModel.Role = userModel.Role;

        if (ModelState.IsValid)
        {
            _context.Update(existingModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var availableUsers = _context.Users
            .Where(u => !_context.UserModel.Select(um => um.IdentityUserId).Contains(u.Id) ||
                        u.Id == existingModel.IdentityUserId)
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        ViewData["IdentityUserId"] = new SelectList(availableUsers, "Id", "UserName", existingModel.IdentityUserId);

        ViewData["Role"] = EnumExtensions.ToSelectList<UserRolesEnum>(existingModel.Role);

        return View(existingModel);
    }

    // GET: User/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var userModel = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Include(u => u.Pages)
            .Include(u => u.Comments)
            .Include(u => u.Ratings)
            .Include(u => u.Entries)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (userModel == null) return NotFound();

        return View(userModel);
    }

    // POST: User/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!UserHasRole()) return Unauthorized();

        var userModel = await _context.UserModel.FindAsync(id);
        if (userModel != null) _context.UserModel.Remove(userModel);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private string? GetCurrentUserIdentifierUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private bool UserHasRole(UserRolesEnum? roleToCheck = null)
    {
        var identifierUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return _context.UserModel
            .Any(u => u.IdentityUser.Id == identifierUserId &&
                      (u.Role == UserRolesEnum.Administrator ||
                       (roleToCheck.HasValue && u.Role == roleToCheck.Value)));
    }
}