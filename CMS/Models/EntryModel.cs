using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class EntryModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Podstrona' jest wymagane.")]
    [Display(Name = "Podstrona")]
    public int PageId { get; set; }

    [ValidateNever]
    public virtual PageModel Page { get; set; }

    [Display(Name = "Użytkownik")] public int? UserId { get; set; }
    [ValidateNever]
    public virtual UserModel? User { get; set; }

    [Required(ErrorMessage = "Pole 'Data stworzenia' jest wymagane.")]
    [DataType(DataType.Date, ErrorMessage = "Wprowadź poprawną datę.")]
    [Display(Name = "Data stworzenia")]
    public DateTime CreatedAt { get; set; }
}