using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROG7312_POE
{
    public partial class Form1 : Form
    {
        private Panel navigationPanel;
        private Panel contentPanel;
        private Button currentButton;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            // Navigation Panel
            navigationPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 64, 122) // Dark blue
            };

            // Logo and Title
            Label titleLabel = new Label
            {
                Text = "MUNICIPALITY SERVICES",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Width = 300,
                Padding = new Padding(20, 0, 0, 0)
            };

            // Navigation buttons
            FlowLayoutPanel navButtonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };

            Button btnReport = CreateNavButton("REPORT ISSUES");
            Button btnEvents = CreateNavButton("EVENTS");
            Button btnStatus = CreateNavButton("SERVICE STATUS");
            Button btnContact = CreateNavButton("CONTACT");

            // Disable buttons that are not implemented
            btnEvents.Enabled = false;
            btnStatus.Enabled = false;

            // Add click events
            btnReport.Click += (s, e) => 
            {
                ActivateButton(s as Button);
                using (var reportForm = new ReportIssueForm())
                {
                    this.Hide();
                    var result = reportForm.ShowDialog(this);
                    this.Show();
                }
            };
            
            btnEvents.Click += (s, e) => ActivateButton(s as Button);
            btnStatus.Click += (s, e) => ActivateButton(s as Button);
            btnContact.Click += (s, e) => ShowContactInfo();

            navButtonsPanel.Controls.AddRange(new Control[] { btnReport, btnEvents, btnStatus, btnContact });

            navigationPanel.Controls.Add(navButtonsPanel);
            navigationPanel.Controls.Add(titleLabel);

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Welcome content
            TableLayoutPanel welcomePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40)
            };

            welcomePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            welcomePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            welcomePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Header
            Label welcomeLabel = new Label
            {
                Text = "Welcome to Municipality Services",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 64, 122),
                TextAlign = ContentAlignment.BottomCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // Description
            Label descriptionLabel = new Label
            {
                Text = "We're here to serve you better. Report issues, check service status, and stay updated with local events.\n\n" +
                      "Our platform provides a seamless way to connect with municipal services and keep your community clean and safe.",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            welcomePanel.Controls.Add(welcomeLabel, 0, 0);
            welcomePanel.Controls.Add(descriptionLabel, 0, 1);

            // Add a footer
            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            Label footerLabel = new Label
            {
                Text = " 2025 Municipality Services. All rights reserved.",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            footer.Controls.Add(footerLabel);

            contentPanel.Controls.Add(welcomePanel);
            contentPanel.Controls.Add(footer);

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);

            // Set initial active button
            ActivateButton(btnReport);
        }

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(150, 70),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 25, 0, 0)
            };
        }

        private void ActivateButton(Button button)
        {
            if (currentButton != null)
            {
                currentButton.BackColor = Color.Transparent;
                currentButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            button.BackColor = Color.FromArgb(0, 95, 179); // Slightly lighter blue
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            currentButton = button;
        }

        private void ShowContactInfo()
        {
            MessageBox.Show("For assistance, please contact:\n\n" +
                          "Email: info@municipality.gov.za\n" +
                          "Phone: 0800 123 456\n" +
                          "Emergency: 112",
                          "Contact Information",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }
    }
}
