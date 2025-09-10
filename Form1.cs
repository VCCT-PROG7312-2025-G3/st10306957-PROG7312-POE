using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROG7312_POE
{
    public partial class Form1 : Form
    {
        private Panel navigationPanel;
        private Panel contentPanel;
        private Panel comingSoonPanel;
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
                Height = 50,  // Reduced height
                BackColor = Color.FromArgb(0, 64, 122), // Dark blue
                Width = 1000  // Fixed width to match form
            };

            // Logo and Title - make it smaller
            Label titleLabel = new Label
            {
                Text = "MUNICIPALITY",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Width = 200,  // Reduced width
                Padding = new Padding(20, 0, 0, 0)
            };

            // Navigation buttons - use a panel with FlowLayout
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 10, 5)  // Add some padding
            };

            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Create buttons with shorter text
            Button btnReport = CreateNavButton("REPORT");
            Button btnEvents = CreateNavButton("EVENTS");
            Button btnStatus = CreateNavButton("STATUS");
            Button btnContact = CreateNavButton("CONTACT");

            // Add buttons to flow panel
            flowPanel.Controls.Add(btnReport);
            flowPanel.Controls.Add(btnEvents);
            flowPanel.Controls.Add(btnStatus);
            flowPanel.Controls.Add(btnContact);

            // Add flow panel to button panel
            buttonPanel.Controls.Add(flowPanel);

            // Add to navigation panel
            navigationPanel.Controls.Add(buttonPanel);
            navigationPanel.Controls.Add(titleLabel);

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
            
            btnEvents.Click += (s, e) => 
            {
                ActivateButton(s as Button);
                MessageBox.Show("Local Events and Announcements\n\nThis feature is coming soon!", "Coming Soon", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            btnStatus.Click += (s, e) => 
            {
                ActivateButton(s as Button);
                MessageBox.Show("Service Request Status\n\nThis feature is coming soon!", "Coming Soon", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            
            btnContact.Click += (s, e) => ShowContactInfo();

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

            // Coming Soon Panel (initially hidden)
            comingSoonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };

            contentPanel.Controls.Add(comingSoonPanel);

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);

            // Set initial active button
            ActivateButton(btnReport);
        }

        private void ShowComingSoon(string featureName)
        {
            // Clear existing controls
            comingSoonPanel.Controls.Clear();

            // Create a table layout for centering
            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Add title
            Label titleLabel = new Label
            {
                Text = featureName,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 64, 122),
                TextAlign = ContentAlignment.BottomCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // Add coming soon message
            Label messageLabel = new Label
            {
                Text = "Coming Soon...",
                Font = new Font("Segoe UI", 36, FontStyle.Italic),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            // Add controls to the panel
            tableLayout.Controls.Add(titleLabel, 0, 0);
            tableLayout.Controls.Add(messageLabel, 0, 1);
            
            comingSoonPanel.Controls.Add(tableLayout);
            comingSoonPanel.Visible = true;
        }

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular), 
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(5, 10, 5, 10), 
                Padding = new Padding(0),
                TextImageRelation = TextImageRelation.TextBeforeImage,
                ImageAlign = ContentAlignment.MiddleCenter
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
