using System.ComponentModel.DataAnnotations;

namespace CMS.Enums;

public enum RatingsEnum
{
    [Display(Name = "Bardzo źle")] Poor = 1,
    [Display(Name = "Źle")] Fair = 2,
    [Display(Name = "Średnio")] Good = 3,
    [Display(Name = "Dobrze")] VeryGood = 4,
    [Display(Name = "Bardzo dobrze")] Excellent = 5
}