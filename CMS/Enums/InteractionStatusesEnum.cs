using System.ComponentModel.DataAnnotations;

namespace CMS.Enums;

public enum InteractionStatusesEnum
{
    [Display(Name = "Niezwerfikowane")] Unverified = 1,
    [Display(Name = "Zaakceptowane")] Accepted = 2,
    [Display(Name = "Odrzucone")] Rejected = 3
}