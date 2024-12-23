using System.ComponentModel.DataAnnotations;

namespace CMS.Enums;

public enum UserRolesEnum
{
    [Display(Name = "Czytelnik")] Reader = 1,
    [Display(Name = "Analityk")] Analyst = 2,
    [Display(Name = "Moderator")] Moderator = 3,
    [Display(Name = "Autor")] Author = 4,
    [Display(Name = "Administrator")] Administrator = 5
}