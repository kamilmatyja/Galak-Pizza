using System.ComponentModel.DataAnnotations;

namespace CMS.Areas;

public class ReservedWordsValidator : ValidationAttribute
{
    private readonly string[] _reservedWords;

    public ReservedWordsValidator(string[] reservedWords)
    {
        _reservedWords = reservedWords;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue)
            if (_reservedWords.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
                return new ValidationResult(
                    $"Pole 'Link' nie może mieć wartości: {string.Join(", ", _reservedWords)}.");

        return ValidationResult.Success;
    }
}