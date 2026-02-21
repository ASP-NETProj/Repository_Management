namespace RepositoryApp.Repository
{
    /// <summary>
    /// Interface for validating item content based on type
    /// </summary>
    public interface IContentValidator
    {
        /// <summary>
        /// Validates content based on the specified type
        /// </summary>
        /// <param name="content">Content to validate</param>
        /// <param name="itemType">Type of content (1=JSON, 2=XML)</param>
        /// <returns>Validation result with success flag and error message</returns>
        ValidationResult Validate(string content, int itemType);
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Fail(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }
}
