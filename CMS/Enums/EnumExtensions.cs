using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Enums;

public static class EnumExtensions
{
    public static SelectList ToSelectList<TEnum>(TEnum? selectedValue = null) where TEnum : struct, Enum
    {
        var items = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(value => new
            {
                Value = value.ToString(),
                Text = value.GetDisplayName()
            });

        return new SelectList(items, "Value", "Text", selectedValue);
    }

    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}