using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using CMS.Data;
using CMS.Enums;

public class PopulateUserFilter : IActionFilter
{
    private readonly ApplicationDbContext _context;

    public PopulateUserFilter(ApplicationDbContext context)
    {
        _context = context;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var userModel = _context.UserModel.FirstOrDefault(u => u.IdentityUser.Id == userId);

        if (context.Controller is Controller controller)
        {
            controller.ViewData["CurrentUserModel"] = userModel;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}