using CMS.Data;
using CMS.Enums;
using CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CMS.Areas.Identity;

public class CustomUserManager : UserManager<IdentityUser>
{
    private readonly ApplicationDbContext _context;

    public CustomUserManager(
        IUserStore<IdentityUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IdentityUser> passwordHasher,
        IEnumerable<IUserValidator<IdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<IdentityUser>> logger,
        ApplicationDbContext context)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
            services, logger)
    {
        _context = context;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
    {
        var result = await base.CreateAsync(user, password);

        if (result.Succeeded)
        {
            var isFirstUser = !_context.UserModel.Any();

            var userModel = new UserModel
            {
                IdentityUserId = user.Id,
                IdentityUser = user,
                CreatedAt = DateTime.UtcNow,
                Role = isFirstUser ? UserRolesEnum.Administrator : UserRolesEnum.Reader
            };

            _context.UserModel.Add(userModel);
            await _context.SaveChangesAsync();
        }

        return result;
    }
}