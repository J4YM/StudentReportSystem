using System.Linq;
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
            var normalized = NormalizePhoneNumberInternal(phoneNumber);
            return !string.IsNullOrEmpty(normalized) && InternationalPhonePattern.IsMatch(normalized);
        }

        /// <summary>
        /// Gets a formatted phone number (preserves original format or adds + if missing)
        /// </summary>
        /// <param name="phoneNumber">The phone number to format</param>
        /// <returns>Formatted phone number in E.164 format</returns>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            var normalized = NormalizePhoneNumberInternal(phoneNumber);
            return normalized ?? string.Empty;
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

            var normalized = NormalizePhoneNumberInternal(phoneNumber);

            if (string.IsNullOrEmpty(normalized))
            {
                return "Invalid phone number format. Please use international format (e.g., +1234567890 or use 0XXXXXXXXXX for Philippines numbers).";
            }

            if (!InternationalPhonePattern.IsMatch(normalized))
            {
                return "Invalid phone number format. Please use international format (e.g., +1234567890).";
            }

            return string.Empty;
        }

        /// <summary>
        /// Normalizes phone numbers into E.164 format, auto-converting Philippines local numbers.
        /// </summary>
        private static string? NormalizePhoneNumberInternal(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            string trimmed = phoneNumber.Trim();
            bool startsWithPlus = trimmed.StartsWith("+");
            bool startsWithZero = trimmed.StartsWith("0");

            string digitsOnly = new string(trimmed.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digitsOnly))
            {
                return null;
            }

            string normalized;

            if (startsWithPlus)
            {
                normalized = "+" + digitsOnly;
            }
            else if (startsWithZero)
            {
                if (digitsOnly.Length < 10)
                {
                    return null; // Not enough digits for a valid PH number
                }

                normalized = "+63" + digitsOnly.Substring(1);
            }
            else
            {
                normalized = "+" + digitsOnly;
            }

            // Basic length check before final regex validation
            if (normalized.Length < 8 || normalized.Length > 16)
            {
                return null;
            }

            return normalized;
        }
    }
}
