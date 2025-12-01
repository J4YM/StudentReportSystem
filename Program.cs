using StudentReportInitial.Forms;
using StudentReportInitial.Data;
using StudentReportInitial.Theming;

namespace StudentReportInitial
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            ThemeManager.Initialize();

            try
            {
                // Initialize database
                await DatabaseHelper.InitializeDatabaseAsync();
                
                // Show login form
                using var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.CurrentUser != null)
                {
                    // Show main dashboard
                    using var dashboard = new MainDashboard(loginForm.CurrentUser);
                    Application.Run(dashboard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}