using System.ComponentModel.DataAnnotations;
using CMS.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class UserModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Użytkownik' jest wymagane.")]
    [Display(Name = "Użytkownik")]
    public string IdentityUserId { get; set; }

    [ValidateNever]
    public IdentityUser IdentityUser { get; set; }

    [Required(ErrorMessage = "Pole 'Data stworzenia' jest wymagane.")]
    [DataType(DataType.Date, ErrorMessage = "Wprowadź poprawną datę.")]
    [Display(Name = "Data stworzenia")]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Pole 'Rola' jest wymagane.")]
    [Display(Name = "Rola")]
    public UserRolesEnum Role { get; set; }

    public virtual ICollection<PageModel> Pages { get; set; } = new List<PageModel>();
    public virtual ICollection<EntryModel> Entries { get; set; } = new List<EntryModel>();
    public virtual ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
    public virtual ICollection<RateModel> Ratings { get; set; } = new List<RateModel>();
}