using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMS.Models;

namespace CMS.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CategoryModel> CategoryModel { get; set; } = default!;

    public DbSet<CommentModel> CommentModel { get; set; } = default!;
    public DbSet<PageModel> PageModel { get; set; } = default!;

    public DbSet<PageContentModel> PageContentModels { get; set; } = default!;

    public DbSet<EntryModel> EntryModel { get; set; } = default!;

    public DbSet<RateModel> RateModel { get; set; } = default!;
    public DbSet<UserModel> UserModel { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCascadeDelete(modelBuilder);
    }

    private void ConfigureCascadeDelete(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            var clrType = entityType.ClrType;
            var properties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    var relatedEntityType = property.PropertyType.GetGenericArguments()[0];
                    var navigationProperties = relatedEntityType
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.PropertyType == clrType);

                    foreach (var navigationProperty in navigationProperties)
                        modelBuilder.Entity(clrType)
                            .HasMany(property.Name)
                            .WithOne(navigationProperty.Name)
                            .OnDelete(DeleteBehavior.Cascade);
                }
        }
    }
}