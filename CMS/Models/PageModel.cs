using System.ComponentModel.DataAnnotations;
using CMS.Areas;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models;

public class PageModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pole 'Użytkownik' jest wymagane.")]
    [Display(Name = "Użytkownik")]
    public int UserId { get; set; }

    [ValidateNever]
    public virtual UserModel User { get; set; }

    [Display(Name = "Podstrona rodzic")] public int? ParentPageId { get; set; }
    [ValidateNever]
    public virtual PageModel? ParentPage { get; set; }

    [Required(ErrorMessage = "Pole 'Kategoria' jest wymagane.")]
    [Display(Name = "Kategoria")]
    public int CategoryId { get; set; }

    [ValidateNever]
    public virtual CategoryModel Category { get; set; }

    [Required(ErrorMessage = "Pole 'Data stworzenia' jest wymagane.")]
    [DataType(DataType.Date, ErrorMessage = "Wprowadź poprawną datę.")]
    [Display(Name = "Data stworzenia")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Link")]
    [RegularExpression("^[a-zA-Z0-9-]*$",
        ErrorMessage = "Pole 'Link' może zawierać tylko litery (a-z, A-Z), cyfry (0-9) i myślniki (-).")]
    [ReservedWordsValidator(new[] { "Category", "Comment", "Entry", "Page", "Rate", "User", "Identity" })]
    [MaxLength(50, ErrorMessage = "Pole 'Link' może mieć maksymalnie 50 znaków.")]
    public string? Link { get; set; }

    [Required(ErrorMessage = "Pole 'Tytuł' jest wymagane.")]
    [MinLength(1, ErrorMessage = "Pole 'Tytuł' musi zawierać co najmniej 1 znak.")]
    [MaxLength(500, ErrorMessage = "Pole 'Tytuł' może zawierać maksymalnie 500 znaków.")]
    [Display(Name = "Tytuł")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Pole 'Treść' jest wymagane.")]
    [MinLength(5, ErrorMessage = "Pole 'Treść' musi zawierać co najmniej 5 znaków.")]
    [MaxLength(5000, ErrorMessage = "Pole 'Treść' może zawierać maksymalnie 5000 znaków.")]
    [Display(Name = "Treść")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Pole 'Słowa kluczowe' jest wymagane.")]
    [MinLength(1, ErrorMessage = "Pole 'Słowa kluczowe' musi zawierać co najmniej 1 znak.")]
    [MaxLength(500, ErrorMessage = "Pole 'Słowa kluczowe' może zawierać maksymalnie 500 znaków.")]
    [Display(Name = "Słowa kluczowe")]
    public string Keywords { get; set; }

    [Required(ErrorMessage = "Pole 'Zdjęcie' jest wymagane.")]
    [Display(Name = "Zdjęcie")]
    public string Image { get; set; }

    public virtual ICollection<EntryModel> Entries { get; set; } = new List<EntryModel>();
    public virtual ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
    public virtual ICollection<RateModel> Ratings { get; set; } = new List<RateModel>();
    public virtual ICollection<PageContentModel> Contents { get; set; } = new List<PageContentModel>();
}