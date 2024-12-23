using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class CategoryModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Użytkownik' jest wymagane.")]
    [Display(Name = "Użytkownik")]
    public int UserId { get; set; }

    [ValidateNever]
    public virtual UserModel User { get; set; }

    [Required(ErrorMessage = "Pole 'Data stworzenia' jest wymagane.")]
    [DataType(DataType.Date, ErrorMessage = "Wprowadź poprawną datę.")]
    [Display(Name = "Data stworzenia")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Pole 'Nazwa' jest wymagane.")]
    [MinLength(1, ErrorMessage = "Pole 'Nazwa' musi zawierać co najmniej 1 znak.")]
    [MaxLength(50, ErrorMessage = "Pole 'Nazwa' może mieć maksymalnie 50 znaków.")]
    [Display(Name = "Nazwa")]
    public string Name { get; set; }

    public virtual ICollection<PageModel> Pages { get; set; } = new List<PageModel>();
}