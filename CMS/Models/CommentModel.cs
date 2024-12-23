using System.ComponentModel.DataAnnotations;
using CMS.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class CommentModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Podstrona' jest wymagane.")]
    [Display(Name = "Podstrona")]
    public int PageId { get; set; }

    [ValidateNever]
    public virtual PageModel Page { get; set; }

    [Required(ErrorMessage = "Pole 'Użytkownik' jest wymagane.")]
    [Display(Name = "Użytkownik")]
    public int UserId { get; set; }

    [ValidateNever]
    public virtual UserModel User { get; set; }

    [Required(ErrorMessage = "Pole 'Data stworzenia' jest wymagane.")]
    [DataType(DataType.Date, ErrorMessage = "Wprowadź poprawną datę.")]
    [Display(Name = "Data stworzenia")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Pole 'Treść' jest wymagane.")]
    [MinLength(1, ErrorMessage = "Pole 'Treść' musi zawierać co najmniej 1 znak.")]
    [MaxLength(500, ErrorMessage = "Pole 'Treść' może mieć maksymalnie 500 znaków.")]
    [Display(Name = "Treść")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Pole 'Status' jest wymagane.")]
    [Display(Name = "Status")]
    public InteractionStatusesEnum Status { get; set; }
}