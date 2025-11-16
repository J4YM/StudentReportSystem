using System.Configuration;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StudentReportInitial.Data
{
    public static class SmsService
    {
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();
        private static DateTime _lastSmsSent = DateTime.MinValue;
        private static readonly TimeSpan _minDelayBetweenSms = TimeSpan.FromSeconds(1); // Rate limiting: 1 second between SMS

        private static void InitializeTwilio()
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (!_isInitialized)
                    {
                        var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"] ?? "";
                        var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"] ?? "";

                        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                        {
                            throw new InvalidOperationException("Twilio API configuration is missing. Please check app.config.");
                        }

                        TwilioClient.Init(accountSid, authToken);
                        _isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sends an SMS message to a phone number using Twilio API
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number (international format)</param>
        /// <param name="message">Message text to send</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        public static async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                {
                    return false;
                }

                // Initialize Twilio client
                InitializeTwilio();

                // Clean phone number (remove spaces, dashes, etc.)
                string cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();
                
                // Ensure E.164 format (with + prefix) - Twilio requires this format
                if (!cleanPhone.StartsWith("+"))
                {
                    // If number starts with 0, assume it's a local number and needs country code
                    // For Philippines: 0XXXXXXXXX -> +63XXXXXXXXX
                    // For other countries, user should provide country code
                    if (cleanPhone.StartsWith("0"))
                    {
                        // Default to Philippines country code if starts with 0
                        cleanPhone = "+63" + cleanPhone.Substring(1);
                    }
                    else
                    {
                        // Assume it's already in international format without +
                        cleanPhone = "+" + cleanPhone;
                    }
                }
                
                // Validate phone number format (E.164: + followed by 1-15 digits)
                if (!cleanPhone.StartsWith("+") || cleanPhone.Length < 8 || cleanPhone.Length > 16)
                {
                    System.Diagnostics.Debug.WriteLine($"SMS: Invalid phone number format: {cleanPhone}");
                    MessageBox.Show(
                        "Invalid phone number format.\n\n" +
                        "Please use international format:\n" +
                        "• +1234567890 (with country code)\n" +
                        "• Or local format starting with 0 (will default to Philippines +63)",
                        "Invalid Phone Number",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                // Rate limiting: Add delay between SMS sends to avoid triggering spam detection
                var timeSinceLastSms = DateTime.Now - _lastSmsSent;
                if (timeSinceLastSms < _minDelayBetweenSms)
                {
                    var delayMs = (int)(_minDelayBetweenSms - timeSinceLastSms).TotalMilliseconds;
                    await Task.Delay(delayMs);
                }

                // Get Twilio phone number from config (required for sending)
                var twilioPhoneNumber = ConfigurationManager.AppSettings["TwilioPhoneNumber"] ?? "";
                if (string.IsNullOrWhiteSpace(twilioPhoneNumber))
                {
                    System.Diagnostics.Debug.WriteLine("SMS: Twilio phone number not configured");
                    MessageBox.Show(
                        "Twilio phone number is not configured in app.config.\n\n" +
                        "Please add your Twilio phone number to the configuration.",
                        "SMS Configuration Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return false;
                }

                // Ensure Twilio phone number is in E.164 format
                if (!twilioPhoneNumber.StartsWith("+"))
                {
                    twilioPhoneNumber = "+" + twilioPhoneNumber;
                }

                System.Diagnostics.Debug.WriteLine($"SMS: Sending to {cleanPhone} from {twilioPhoneNumber}");

                // Send SMS using Twilio API - using CreateMessageOptions pattern
                var messageOptions = new CreateMessageOptions(new PhoneNumber(cleanPhone))
                {
                    From = new PhoneNumber(twilioPhoneNumber),
                    Body = message
                };

                var messageResource = await MessageResource.CreateAsync(messageOptions);

                _lastSmsSent = DateTime.Now; // Update last sent time

                // Check message status
                var status = messageResource.Status;
                var sid = messageResource.Sid;
                var errorCode = messageResource.ErrorCode;
                var errorMessage = messageResource.ErrorMessage;

                System.Diagnostics.Debug.WriteLine($"SMS Response - SID: {sid}, Status: {status}, ErrorCode: {errorCode}, ErrorMessage: {errorMessage}");

                // Check for errors
                if (errorCode.HasValue && errorCode.Value != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"SMS Error - Code: {errorCode}, Message: {errorMessage}");
                    MessageBox.Show(
                        $"SMS sending failed!\n\n" +
                        $"Error Code: {errorCode}\n" +
                        $"Error Message: {errorMessage}\n\n" +
                        $"Common issues:\n" +
                        $"• Invalid phone number format\n" +
                        $"• Insufficient account balance\n" +
                        $"• Phone number not verified (for trial accounts)\n" +
                        $"• Account restrictions\n\n" +
                        $"Check your Twilio dashboard for details.",
                        "SMS Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                // Success statuses: queued, sending, sent, delivered
                var successStatuses = new[] { "queued", "sending", "sent", "delivered" };
                bool success = successStatuses.Contains(status?.ToString().ToLower() ?? "");

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"SMS Status: {status} (not a success status)");
                }

                return success;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - SMS failures shouldn't break the application
                System.Diagnostics.Debug.WriteLine($"SMS sending failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SMS Exception Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"SMS Stack Trace: {ex.StackTrace}");

                MessageBox.Show(
                    $"SMS sending failed: {ex.Message}\n\n" +
                    $"Check Debug output for more details.",
                    "SMS Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

        /// <summary>
        /// Generates a 6-digit OTP code
        /// </summary>
        public static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Sends an OTP code to a phone number
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number</param>
        /// <param name="otpCode">OTP code to send</param>
        /// <returns>True if sent successfully</returns>
        public static async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
        {
            string message = $"Your STI BALIUAG verification code is: {otpCode}. Valid for 10 minutes. Do not share this code with anyone.";
            return await SendSmsAsync(phoneNumber, message);
        }

        /// <summary>
        /// Sends a grade notification to student/guardian
        /// </summary>
        public static async Task<bool> SendGradeNotificationAsync(string phoneNumber, string studentName, string subjectName, string assignmentType, decimal score, decimal maxScore, string professorName)
        {
            string message = $"Grade Update - {studentName}\n" +
                           $"Subject: {subjectName}\n" +
                           $"Type: {assignmentType}\n" +
                           $"Score: {score}/{maxScore}\n" +
                           $"Instructor: {professorName}\n" +
                           $"STI BALIUAG";
            
            return await SendSmsAsync(phoneNumber, message);
        }

        /// <summary>
        /// Sends an attendance notification to guardian
        /// </summary>
        public static async Task<bool> SendAttendanceNotificationAsync(string phoneNumber, string studentName, string subjectName, string status, DateTime date, string professorName, int attendanceCount)
        {
            string statusText = status.ToUpper() == "PRESENT" ? "Present" : 
                               status.ToUpper() == "ABSENT" ? "Absent" :
                               status.ToUpper() == "LATE" ? "Late" : "Excused";
            
            string message = $"Attendance Update - {studentName}\n" +
                           $"Subject: {subjectName}\n" +
                           $"Status: {statusText}\n" +
                           $"Date: {date:MMM dd, yyyy hh:mm tt}\n" +
                           $"Total Attendance Count: {attendanceCount}\n" +
                           $"Instructor: {professorName}\n" +
                           $"STI BALIUAG";
            
            return await SendSmsAsync(phoneNumber, message);
        }
    }
}
