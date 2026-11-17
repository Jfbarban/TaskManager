using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class FechaNoPasadaAttribute : ValidationAttribute
{
    public FechaNoPasadaAttribute()
        : base("La fecha límite no puede estar en el pasado.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Si es null, de eso se encarga [Required]
        if (value is null) return ValidationResult.Success;

        if (value is DateOnly fecha && fecha < DateOnly.FromDateTime(DateTime.Today))
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });

        return ValidationResult.Success;
    }
}