using System.ComponentModel.DataAnnotations;

namespace AppCoel.Models.CustomValidators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidateCollectionAttribute :  ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var results = new List<ValidationResult>();

            if (value is IEnumerable<object> collection)
            {
                foreach ( var item in collection)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var context = new ValidationContext(item);
                    Validator.TryValidateObject(item, context, results, validateAllProperties: true);
                }
            }
            else
            {
                var context = new ValidationContext(value);
                Validator.TryValidateObject(value, context, results, validateAllProperties: true);
            }

            if (results.Count > 0)
            {
                var erros = results.Select(x => x.ErrorMessage).ToList();
                return new ValidationResult(string.Join("; ", erros));
            }

            return ValidationResult.Success;
        }
    }
}
