using System.ComponentModel.DataAnnotations;
using CMS.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class PageContentModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Podstrona' jest wymagane.")]
    [Display(Name = "Podstrona")]
    public int PageId { get; set; }

    [ValidateNever]
    public virtual PageModel Page { get; set; }

    [Required(ErrorMessage = "Pole 'Typ' jest wymagane.")]
    [Display(Name = "Typ")]
    public ContentTypesEnum Type { get; set; }

    [Required(ErrorMessage = "Pole 'Zawartość' jest wymagane.")]
    [MinLength(1, ErrorMessage = "Pole 'Zawartość' musi zawierać co najmniej 1 znak.")]
    [MaxLength(5000, ErrorMessage = "Pole 'Zawartość' może zawierać maksymalnie 5000 znaków.")]
    [Display(Name = "Zawartość")]
    public string Value { get; set; }

    [Required(ErrorMessage = "Pole 'Kolejność' jest wymagane.")]
    [Range(1, int.MaxValue, ErrorMessage = "Pole 'Kolejność' musi być liczbą większą od 0.")]
    [Display(Name = "Kolejność")]
    public int Order { get; set; }
}