using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using PROG7312_POE.Data;
using PROG7312_POE.Services;

namespace PROG7312_POE.Forms
{
    public partial class MainForm : Form
    {
        private readonly IIssueService _issueService;
        private readonly IServiceProvider _serviceProvider;
        private Button currentButton;
        private Panel contentPanel;
        private Panel navigationPanel;
        private Button btnReport;
        private Button btnEvents;
        private Button btnStatus;

        public MainForm() : this(
            new IssueService(new IssueRepository(), new FileService()),
            new ServiceCollection()
                .AddScoped<IIssueRepository, IssueRepository>()
                .AddScoped<IFileService, FileService>()
                .AddScoped<IIssueService, IssueService>()
                .BuildServiceProvider())
        {
        }

        public MainForm(IIssueService issueService, IServiceProvider serviceProvider)
        {
            _issueService = issueService;
            _serviceProvider = serviceProvider;
            InitializeComponent();
            InitializeMainMenu();
        }

        private void InitializeMainMenu()
        {
            // Form settings
            this.Text = "Municipality Services";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(0);

            // Main content panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Navigation Panel
            navigationPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 64, 122),
                Padding = new Padding(0, 0, 20, 0)  // Right padding
            };

            // Title Label
            Label titleLabel = new Label
            {
                Text = "MUNICIPALITY",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 0),
                Size = new Size(200, 60)
            };

            // Create navigation buttons
            btnReport = CreateNavButton("Report");
            btnEvents = CreateNavButton("Events");
            btnStatus = CreateNavButton("Status");

            // Set button properties
            btnReport.Click += (s, e) => ShowReportForm();
            btnEvents.Click += (s, e) => ShowComingSoon("Events & Announcements");
            btnStatus.Click += (s, e) => ShowComingSoon("Service Status");

            // Add controls to navigation panel
            navigationPanel.Controls.Add(btnStatus);
            navigationPanel.Controls.Add(btnEvents);
            navigationPanel.Controls.Add(btnReport);
            navigationPanel.Controls.Add(titleLabel);

            // Welcome content
            Label welcomeLabel = new Label
            {
                Text = "Welcome to Municipal Services",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 64, 122),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Add panels to form
            contentPanel.Controls.Add(welcomeLabel);
            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);

            // Set initial active button
            ActivateButton(btnReport);

            // Handle window resizing
            this.Resize += (s, e) => PositionButtons();

            // Initial button positioning
            PositionButtons();
        }

        private void PositionButtons()
        {
            if (btnReport == null || btnEvents == null || btnStatus == null)
                return;

            int buttonWidth = 100;
            int buttonSpacing = 10;
            int rightEdge = this.ClientSize.Width - 30;  // 30px from right edge

            btnReport.Location = new Point(rightEdge - buttonWidth, 12);
            btnEvents.Location = new Point(btnReport.Left - buttonWidth - buttonSpacing, 12);
            btnStatus.Location = new Point(btnEvents.Left - buttonWidth - buttonSpacing, 12);
        }

        private void ShowReportForm()
        {
            try
            {
                // Get the required services
                var issueService = _serviceProvider.GetRequiredService<IIssueService>();
                var fileService = _serviceProvider.GetRequiredService<IFileService>();

                using (var reportForm = new ReportIssueForm(issueService, fileService))
                {
                    reportForm.StartPosition = FormStartPosition.CenterParent;
                    var result = reportForm.ShowDialog(this);
                    // Handle the result if needed
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening report form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = {
                    BorderSize = 0,
                    MouseOverBackColor = Color.FromArgb(0, 95, 179)
                },
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(0),
                AutoSize = false
            };
        }

        private void ActivateButton(Button button)
        {
            if (currentButton != null)
            {
                currentButton.BackColor = Color.Transparent;
                currentButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            button.BackColor = Color.FromArgb(0, 95, 179);
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            currentButton = button;
        }

        private void ShowComingSoon(string featureName)
        {
            contentPanel.Controls.Clear();

            Label message = new Label
            {
                Text = $"{featureName}\n\nComing Soon...",
                Font = new Font("Segoe UI", 24, FontStyle.Italic),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            contentPanel.Controls.Add(message);
        }
    }
}