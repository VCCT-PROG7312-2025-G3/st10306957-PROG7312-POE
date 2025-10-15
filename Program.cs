using Microsoft.Extensions.DependencyInjection;
using PROG7312_POE.Data;
using PROG7312_POE.Forms;
using PROG7312_POE.Services;
using System;
using System.Windows.Forms;

namespace PROG7312_POE
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Setup DI
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // Run the main form
                using (var mainForm = serviceProvider.GetRequiredService<MainForm>())
                {
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start the application: {ex.Message}", 
                    "Startup Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            // Register forms
            services.AddScoped<MainForm>();
            services.AddScoped<ReportIssueForm>();
            services.AddScoped<EventsForm>();
        }
    }
}