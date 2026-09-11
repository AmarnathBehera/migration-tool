namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class ValidationResult
    {
        internal static readonly ValidationResult Success = new ValidationResult { IsValid = true };

        public ValidationResult()
        {
        }

        public ValidationResult(string v)
        {
            // When constructed with a message, consider the validation failed.
            ErrorMessage = v;
            IsValid = false;
        }

        /// <summary>
        /// <c>True</c> if the validation succeeded.
        /// </summary>
        public bool IsValid { get; set; }


        /// <summary>
        /// Contains a validation message if the validation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
