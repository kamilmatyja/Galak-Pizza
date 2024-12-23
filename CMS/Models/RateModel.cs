using System.ComponentModel.DataAnnotations;
using CMS.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class RateModel
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

    [Required(ErrorMessage = "Pole 'Wartość' jest wymagane.")]
    [Display(Name = "Wartość")]
    public RatingsEnum Rating { get; set; }

    [Required(ErrorMessage = "Pole 'Status' jest wymagane.")]
    [Display(Name = "Status")]
    public InteractionStatusesEnum Status { get; set; }
}