namespace PowerConsole
{
    /// <summary>
    /// Encapsulates validation messages for type categories.
    /// </summary>
    public sealed class ValidationMessages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessages"/> class.
        /// </summary>
        public ValidationMessages()
        {
        }

        /// <summary>
        /// Represents the default <see cref="ValidationMessages"/> instance.
        /// </summary>
        public static readonly ValidationMessages Default = new ValidationMessages
        {
            ForBoolean = "A logical (true or false) value is required",
            ForDateTime = "A date and/or time value is required",
            ForIntegralNumber = "An integral (whole) number is required",
            ForFloatingPointNumber = "A floating-point (decimal) number is required",
            ForOther = "A value is required"
        };

        /// <summary>
        /// Gets or sets the validation for a type of the <see cref="TypeCategory.Boolean"/> category.
        /// </summary>
        public string ForBoolean { get; set; }

        /// <summary>
        /// Gets or sets the validation for a type of the <see cref="TypeCategory.DateTime"/> category.
        /// </summary>
        public string ForDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the validation for a type of the <see cref="TypeCategory.IntegralNumber"/> category.
        /// </summary>
        public string ForIntegralNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the validation for a type of the <see cref="TypeCategory.FloatingPointNumber"/> category.
        /// </summary>
        public string ForFloatingPointNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the validation for a type of the <see cref="TypeCategory.Other"/> category.
        /// </summary>
        public string ForOther { get; set; }

        /// <summary>
        /// Returns the most appropriate error message for the specified type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to return an error message.</typeparam>
        /// <param name="messages">The <see cref="ValidationMessages"/> to use.
        /// If null, <see cref="Default"/> will be used.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static string GetDefaultValidationMessage<T>(ValidationMessages messages = null)
        {
            if (messages == null)
                messages = Default;

            string validationMessage;

            switch (typeof(T).GetTypeCategory())
            {
                case TypeCategory.Boolean:
                    validationMessage = messages.ForBoolean;
                    break;
                case TypeCategory.DateTime:
                    validationMessage = messages.ForDateTime;
                    break;
                case TypeCategory.IntegralNumber:
                    validationMessage = messages.ForIntegralNumber;
                    break;
                case TypeCategory.FloatingPointNumber:
                    validationMessage = messages.ForFloatingPointNumber;
                    break;
                case TypeCategory.Other:
                default:
                    validationMessage = messages.ForOther;
                    break;
            }

            return $"#ERROR: Invalid input: {validationMessage}! Try again. ";
        }
    }
}
