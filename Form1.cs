using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROG7312_POE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeMainMenu();
        }

        private void InitializeMainMenu()
        {
            // Form settings
            this.Text = "Municipality Services";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Main panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Title label
            Label titleLabel = new Label
            {
                Text = "Welcome to Municipality Services",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            // Button panel
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(50, 20, 50, 20)
            };

            // Create buttons
            Button btnReportIssues = CreateMenuButton("Report Issues");
            Button btnLocalEvents = CreateMenuButton("Local Events and Announcements");
            Button btnServiceStatus = CreateMenuButton("Service Request Status");

            // Disable buttons that are not implemented yet
            btnLocalEvents.Enabled = false;
            btnServiceStatus.Enabled = false;

            // Add click event for Report Issues button
            btnReportIssues.Click += (s, e) =>
            {
                var reportForm = new ReportIssueForm();
                reportForm.ShowDialog();
            };

            // Add controls to panels
            buttonPanel.Controls.Add(btnReportIssues);
            buttonPanel.Controls.Add(btnLocalEvents);
            buttonPanel.Controls.Add(btnServiceStatus);

            // Add panels to form
            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(titleLabel);
            this.Controls.Add(mainPanel);
        }

        private Button CreateMenuButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(400, 60),
                Margin = new Padding(10),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = true
            };
        }
    }
}
