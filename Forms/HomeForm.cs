using System;
using System.Drawing;
using System.Windows.Forms;

namespace PROG7312_POE.Forms
{
    public partial class HomeForm : UserControl
    {
        private TableLayoutPanel mainLayout;
        private Panel headerPanel;
        private Label lblWelcome;
        private Label lblSubtitle;
        private FlowLayoutPanel cardsPanel;
        private MainForm mainForm;

        // Card colors
        private readonly Color[] cardColors = 
        {
            Color.FromArgb(70, 130, 180),   // Steel Blue
            Color.FromArgb(60, 179, 113),   // Medium Sea Green
            Color.FromArgb(255, 165, 0),    // Orange
            Color.FromArgb(147, 112, 219)   // Medium Purple
        };

        // Card data
        private readonly (string Title, string Description, string Icon)[] cardData = 
        {
            ("Report Issue", "Report a new issue or problem", "📝"),
            ("View Reports", "Track your reported issues", "🔍"),
            ("Events", "View upcoming community events", "📅"),
            ("Status", "Check service request status", "✅")
        };

        public HomeForm(MainForm mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
        }

        private void InitializeComponent()
        {
            // Main layout
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 20f),
                    new RowStyle(SizeType.Percent, 80f)
                }
            };

            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 20, 40, 20)
            };

            lblWelcome = new Label
            {
                Text = "Welcome to Municipal Services",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(0, 20)
            };

            lblSubtitle = new Label
            {
                Text = "How can we assist you today?",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(102, 102, 102),
                AutoSize = true,
                Location = new Point(0, 70)
            };

            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Controls.Add(lblSubtitle);

            // Cards panel
            cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30, 0, 30, 30),
                BackColor = Color.White
            };

            // Create cards
            for (int i = 0; i < cardData.Length; i++)
            {
                var card = CreateCard(cardData[i].Title, cardData[i].Description, cardData[i].Icon, cardColors[i]);
                cardsPanel.Controls.Add(card);
            }

            // Add panels to main layout
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(cardsPanel, 0, 1);

            this.SuspendLayout();
            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private Panel CreateCard(string title, string description, string icon, Color color)
        {
            var card = new Panel
            {
                Width = 250,
                Height = 200,
                Margin = new Padding(20),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = title
            };

            // Simple border
            card.BorderStyle = BorderStyle.FixedSingle;

            // Add hover effect
            card.MouseEnter += (s, e) =>
            {
                card.SuspendLayout();
                card.Width = 260;
                card.Height = 210;
                card.Margin = new Padding(15, 15, 25, 25);
                card.ResumeLayout();
            };

            card.MouseLeave += (s, e) =>
            {
                card.SuspendLayout();
                card.Width = 250;
                card.Height = 200;
                card.Margin = new Padding(20);
                card.ResumeLayout();
            };

            // Add icon
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 32, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Add title
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 70),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            // Add description
            var descLabel = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = false,
                Size = new Size(210, 40),
                Location = new Point(20, 100),
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            // Add click handler
            card.Click += (s, e) => NavigateToForm(title);
            iconLabel.Click += (s, e) => NavigateToForm(title);
            titleLabel.Click += (s, e) => NavigateToForm(title);
            descLabel.Click += (s, e) => NavigateToForm(title);

            // Add controls to card
            card.Controls.Add(iconLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(descLabel);

            return card;
        }

        private void NavigateToForm(string formName)
        {
            switch (formName)
            {
                case "Report Issue":
                    mainForm.ShowReportForm();
                    break;
                case "View Reports":
                    mainForm.ShowReportedIssues();
                    break;
                case "Events":
                    var method = mainForm.GetType().GetMethod("ShowEventsForm", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    method?.Invoke(mainForm, null);
                    break;
                case "Status":
                    var statusMethod = mainForm.GetType().GetMethod("ShowStatusForm",
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    statusMethod?.Invoke(mainForm, null);
                    break;
            }
        }
    }

}
