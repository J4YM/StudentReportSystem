using System.Text.RegularExpressions;

namespace StudentReportInitial.Data
{
    public static class PhoneValidator
    {
        // International phone number pattern (E.164 format)
        // Accepts: +1234567890 or 1234567890 (with country code)
        // Minimum 7 digits, maximum 15 digits (E.164 standard)
        private static readonly Regex InternationalPhonePattern = new Regex(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled);

        /// <summary>
        /// Validates if a phone number is a valid international mobile number
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidPhilippinesMobile(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            // Remove any spaces, dashes, or other formatting
            string cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();

            // Check if it matches the international phone pattern
            return InternationalPhonePattern.IsMatch(cleaned);
        }

        /// <summary>
        /// Gets a formatted phone number (preserves original format or adds + if missing)
        /// </summary>
        /// <param name="phoneNumber">The phone number to format</param>
        /// <returns>Formatted phone number in E.164 format</returns>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            // Remove any spaces, dashes, or other formatting
            string cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();

            // Ensure it starts with + for E.164 format
            if (!cleaned.StartsWith("+"))
            {
                cleaned = "+" + cleaned;
            }

            return cleaned;
        }

        /// <summary>
        /// Gets the validation error message for a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate</param>
        /// <returns>Error message if invalid, empty string if valid</returns>
        public static string GetValidationMessage(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return "Phone number is required.";
            }

            string cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();

            // Check if it's a valid international format
            if (!InternationalPhonePattern.IsMatch(cleaned))
            {
                return "Invalid phone number format. Please use international format (e.g., +1234567890 or with country code).";
            }

            return string.Empty;
        }
    }
}
