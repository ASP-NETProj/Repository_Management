using System.Text.Json;
using System.Xml.Linq;

namespace RepositoryApp.Repository
{
    /// <summary>
    /// Default content validator implementation
    /// </summary>
    public class DefaultContentValidator : IContentValidator
    {
        public ValidationResult Validate(string content, int itemType)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return ValidationResult.Fail("Content cannot be null or empty");
            }

            switch (itemType)
            {
                case 1: // JSON
                    return ValidateJson(content);
                case 2: // XML
                    return ValidateXml(content);
                default:
                    return ValidationResult.Fail($"Unknown item type: {itemType}");
            }
        }

        private ValidationResult ValidateJson(string content)
        {
            // Validation logic placeholder - can be implemented by extending this class
            try
            {
                using var doc = JsonDocument.Parse(content);
                // Additional JSON validation logic would go here
                // For example: schema validation, required fields check, etc.
                return ValidationResult.Success();
            }
            catch (JsonException ex)
            {
                return ValidationResult.Fail($"Invalid JSON format: {ex.Message}");
            }
        }

        private ValidationResult ValidateXml(string content)
        {
            // Validation logic placeholder - can be implemented by extending this class
            try
            {
                var doc = XElement.Parse(content);
                // Additional XML validation logic would go here
                // For example: XSD validation, required elements check, etc.
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Fail($"Invalid XML format: {ex.Message}");
            }
        }
    }
}
