using System.ComponentModel.DataAnnotations;

namespace CMS.Enums;

public enum ContentTypesEnum
{
    [Display(Name = "Tekst")] Text = 1,
    [Display(Name = "Zdjęcie")] Image = 2
}