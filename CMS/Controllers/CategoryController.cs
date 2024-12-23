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
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Category
    public async Task<IActionResult> Index(string? name, int? userId)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        var query = _context.CategoryModel
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(c => c.Pages)
            .AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{name}%"));
        }

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        var users = await _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToListAsync();

        ViewData["FilterUser"] = new SelectList(users, "Id", "UserName", userId);
        ViewData["FilterName"] = name;

        return View(await query.ToListAsync());
    }

    // GET: Category/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!UserHasRole(UserRolesEnum.Analyst)) return Unauthorized();

        if (id == null) return NotFound();

        var categoryModel = await _context.CategoryModel
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(c => c.Pages)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (categoryModel == null) return NotFound();

        return View(categoryModel);
    }

    // GET: Category/Create
    public IActionResult Create()
    {
        if (!UserHasRole()) return Unauthorized();

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", GetUserId());

        var categoryModel = new CategoryModel();
        categoryModel.CreatedAt = DateTime.Now;

        return View(categoryModel);
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,CreatedAt,Name")] CategoryModel categoryModel)
    {
        if (!UserHasRole()) return Unauthorized();

        if (!await _context.UserModel.AnyAsync(u => u.Id == categoryModel.UserId))
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        else
            categoryModel.User = await _context.UserModel.FindAsync(categoryModel.UserId);

        if (await _context.CategoryModel.AnyAsync(c => c.Name == categoryModel.Name))
            ModelState.AddModelError("Name", "Nazwa kategorii test już zajęta.");

        if (ModelState.IsValid)
        {
            _context.Add(categoryModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", categoryModel.UserId);

        return View(categoryModel);
    }

    // GET: Category/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var categoryModel = await _context.CategoryModel.FindAsync(id);
        if (categoryModel == null) return NotFound();

        var users = _context.UserModel
            .Include(u => u.IdentityUser)
            .Select(u => new { u.Id, UserName = u.IdentityUser.UserName })
            .ToList();

        ViewData["UserId"] = new SelectList(users, "Id", "UserName", categoryModel.UserId);

        return View(categoryModel);
    }

    // POST: Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CreatedAt,Name")] CategoryModel categoryModel)
    {
        if (!UserHasRole()) return Unauthorized();

        var existingModel = await _context.CategoryModel.FirstOrDefaultAsync(m => m.Id == id);
        if (existingModel == null) return NotFound();

        if (!await _context.UserModel.AnyAsync(u => u.Id == categoryModel.UserId))
        {
            ModelState.AddModelError("UserId", "Błędny użytkownik.");
        }
        else
        {
            existingModel.UserId = categoryModel.UserId;
            existingModel.User = await _context.UserModel.FindAsync(categoryModel.UserId);
        }

        if (await _context.CategoryModel.AnyAsync(c => c.Name == categoryModel.Name && c.Id != categoryModel.Id))
            ModelState.AddModelError("Name", "Nazwa kategorii test już zajęta.");

        existingModel.CreatedAt = categoryModel.CreatedAt;
        existingModel.Name = categoryModel.Name;

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

        return View(existingModel);
    }

    // GET: Category/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (!UserHasRole()) return Unauthorized();

        if (id == null) return NotFound();

        var categoryModel = await _context.CategoryModel
            .Include(c => c.User)
            .ThenInclude(u => u.IdentityUser)
            .Include(c => c.Pages)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (categoryModel == null) return NotFound();

        return View(categoryModel);
    }

    // POST: Category/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!UserHasRole()) return Unauthorized();

        var categoryModel = await _context.CategoryModel.FindAsync(id);
        if (categoryModel != null) _context.CategoryModel.Remove(categoryModel);

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
}