using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using PROG7312_POE.Data;
using PROG7312_POE.Models;
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
        private Button btnHome;
        private Button btnReport;
        private Button btnEvents;
        private Button btnStatus;

        public MainForm() : this(
            new IssueService(
                new IssueRepository(), 
                new FileService(),
                new RequestPriorityService()
            ),
            new ServiceCollection()
                .AddScoped<IIssueRepository, IssueRepository>()
                .AddScoped<IFileService, FileService>()
                .AddScoped<RequestPriorityService>()
                .AddScoped<IIssueService, IssueService>()
                .AddScoped<IEventService, EventService>()
                .AddScoped<IRecommendationService, RecommendationService>()
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
            this.Size = new Size(1100, 750); // Slightly larger default size
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(0);

            // Main content panel with padding
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Navigation Panel
            navigationPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,  // Increased height
                BackColor = Color.FromArgb(0, 64, 122),
                Padding = new Padding(0, 0, 30, 0)  // Increased right padding
            };

            // Title Label with better spacing
            Label titleLabel = new Label
            {
                Text = "MUNICIPALITY",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(25, 0),
                AutoSize = true,
                Height = 70,
                Padding = new Padding(0, 0, 0, 5)  // Add some bottom padding
            };

            // Create navigation buttons with consistent sizing
            btnHome = CreateNavButton("Home");
            btnReport = CreateNavButton("Report");
            btnEvents = CreateNavButton("Events");
            btnStatus = CreateNavButton("Status");
            var btnViewIssues = CreateNavButton("View Issues");
            var btnPriorityQueue = CreateNavButton("Priority"); // Shorter text for better fit

            // Set button properties
            btnHome.Click += (s, e) => ShowHomeForm();
            btnReport.Click += (s, e) => ShowReportForm();
            btnEvents.Click += (s, e) => ShowEventsForm();
            btnStatus.Click += (s, e) => ShowStatusForm();
            btnViewIssues.Click += (s, e) => ShowReportedIssues();
            btnPriorityQueue.Click += (s, e) => ShowPriorityQueueForm();

            // Add controls to navigation panel
            navigationPanel.Controls.Add(btnPriorityQueue);
            navigationPanel.Controls.Add(btnViewIssues);
            navigationPanel.Controls.Add(btnStatus);
            navigationPanel.Controls.Add(btnEvents);
            navigationPanel.Controls.Add(btnReport);
            navigationPanel.Controls.Add(btnHome);
            navigationPanel.Controls.Add(titleLabel);

            // Create main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.White
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Create home control as default content
            var homeControl = new HomeForm(this);
            mainContainer.Controls.Add(homeControl, 0, 1);

            // Add panels to form
            contentPanel.Controls.Add(mainContainer);
            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);

            // Show home form by default
            ShowHomeForm();

            // Handle window resizing with proper padding
            this.Resize += (s, e) => PositionButtons(30);

            // Initial button positioning with right padding
            PositionButtons(30);
        }

        private async void LoadReportedIssues(FlowLayoutPanel container)
        {
            try
            {
                // Clear existing controls (if any)
                container.Controls.Clear();

                // Get all issues (in a real app, you might want to filter by current user)
                var issues = await _issueService.GetIssuesAsync();

                if (issues == null || !System.Linq.Enumerable.Any(issues))
                {
                    var noIssuesLabel = new Label
                    {
                        Text = "No reported issues found.",
                        Font = new Font("Segoe UI", 12, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        AutoSize = true,
                        Margin = new Padding(0, 20, 0, 0)
                    };
                    container.Controls.Add(noIssuesLabel);
                    return;
                }

                foreach (var issue in issues)
                {
                    var issueCard = new Panel
                    {
                        Width = container.ClientSize.Width - 40, // Account for scrollbar
                        Height = 120,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(0, 0, 0, 15),
                        Padding = new Padding(15),
                        Cursor = Cursors.Hand
                    };

                    // Add hover effect
                    issueCard.MouseEnter += (s, e) => issueCard.BackColor = Color.FromArgb(245, 245, 245);
                    issueCard.MouseLeave += (s, e) => issueCard.BackColor = Color.White;

                    // Create layout for issue card
                    var cardLayout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 2,
                        RowCount = 3,
                        CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                    };
                    cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
                    cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                    cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

                    // Add issue title
                    var titleLabel = new Label
                    {
                        Text = issue.Title,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoEllipsis = true,
                        Dock = DockStyle.Fill,
                        ForeColor = Color.FromArgb(0, 64, 122)
                    };

                    // Add status with colored background
                    var statusLabel = new Label
                    {
                        Text = issue.Status,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        ForeColor = Color.White,
                        BackColor = GetStatusColor(issue.Status),
                        AutoSize = false,
                        Height = 25,
                        Width = 100,
                        Margin = new Padding(0, 2, 0, 0)
                    };

                    // Add description
                    var descLabel = new Label
                    {
                        Text = issue.Description,
                        Font = new Font("Segoe UI", 10),
                        TextAlign = ContentAlignment.TopLeft,
                        AutoEllipsis = true,
                        Dock = DockStyle.Fill,
                        MaximumSize = new Size(0, 40),
                        ForeColor = Color.FromArgb(64, 64, 64)
                    };
                    cardLayout.SetColumnSpan(descLabel, 2);

                    // Add date and location
                    var dateLocationPanel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.LeftToRight,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink
                    };

                    var dateLabel = new Label
                    {
                        Text = issue.ReportedDate.ToString("dd MMM yyyy"),
                        Font = new Font("Segoe UI", 8, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        AutoSize = true,
                        Margin = new Padding(0, 0, 15, 0)
                    };

                    var locationLabel = new Label
                    {
                        Text = $"📍 {issue.Location}",
                        Font = new Font("Segoe UI", 8),
                        ForeColor = Color.Gray,
                        AutoSize = true
                    };

                    dateLocationPanel.Controls.Add(dateLabel);
                    dateLocationPanel.Controls.Add(locationLabel);

                    // Add controls to card layout
                    cardLayout.Controls.Add(titleLabel, 0, 0);
                    cardLayout.Controls.Add(statusLabel, 1, 0);
                    cardLayout.Controls.Add(descLabel, 0, 1);
                    cardLayout.SetColumnSpan(descLabel, 2);
                    cardLayout.Controls.Add(dateLocationPanel, 0, 2);
                    cardLayout.SetColumnSpan(dateLocationPanel, 2);

                    // Add click handler to view issue details
                    issueCard.Click += (s, e) => ShowIssueDetails(issue);
                    foreach (Control control in cardLayout.Controls)
                    {
                        control.Click += (s, e) => ShowIssueDetails(issue);
                        control.Cursor = Cursors.Hand;
                    }

                    // Add card to container
                    issueCard.Controls.Add(cardLayout);
                    container.Controls.Add(issueCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading issues: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status))
                return Color.Gray;

            switch (status.ToLower())
            {
                case "reported":
                    return Color.FromArgb(33, 150, 243);  // Blue
                case "in progress":
                    return Color.FromArgb(255, 152, 0);   // Orange
                case "resolved":
                    return Color.FromArgb(76, 175, 80);   // Green
                case "closed":
                    return Color.FromArgb(158, 158, 158); // Gray
                default:
                    return Color.Gray;
            }
        }

        private void ShowIssueDetails(PROG7312_POE.Models.Issue issue)
        {
            if (issue == null) return;

            // Create and show a dialog with issue details
            var detailsForm = new Form
            {
                Text = "Issue Details",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 120),
                    new ColumnStyle(SizeType.Percent, 100)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 100),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40)
                }
            };

            // Add issue details
            AddDetailRow(layout, "Title:", issue.Title, 0);
            AddDetailRow(layout, "Status:", issue.Status, 1);
            AddDetailRow(layout, "Location:", issue.Location, 2);
            AddDetailRow(layout, "Category:", issue.Category, 3);
            AddDetailRow(layout, "Reported On:", issue.ReportedDate.ToString("f"), 4);
            
            // Add description (multi-line)
            var descLabel = new TextBox
            {
                Text = issue.Description,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 10)
            };
            layout.Controls.Add(new Label { Text = "Description:", TextAlign = ContentAlignment.TopRight, Dock = DockStyle.Fill }, 0, 5);
            layout.Controls.Add(descLabel, 1, 5);
            layout.SetRowSpan(descLabel, 3);

            // Add close button
            var closeButton = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(0, 64, 122),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.FlatAppearance.BorderSize = 0;

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttonPanel.Controls.Add(closeButton);
            closeButton.Width = 100;
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Add controls to form
            detailsForm.Controls.Add(layout);
            detailsForm.Controls.Add(buttonPanel);

            // Show dialog
            detailsForm.ShowDialog(this);
        }

        private void AddDetailRow(TableLayoutPanel layout, string label, string value, int row)
        {
            var labelCtrl = new Label
            {
                Text = label,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            var valueCtrl = new Label
            {
                Text = value,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(10, 0, 0, 0)
            };

            layout.Controls.Add(labelCtrl, 0, row);
            layout.Controls.Add(valueCtrl, 1, row);
        }
        

        private void PositionButtons(int rightPadding = 30)
        {
            if (navigationPanel == null) return;

            int buttonWidth = 100;
            int buttonSpacing = 10;
            int buttonHeight = 40;
            int topMargin = (navigationPanel.Height - buttonHeight) / 2;

            // Start positioning buttons from the right
            int rightPosition = navigationPanel.Width - rightPadding;

            // List of buttons in order from right to left
            var buttons = navigationPanel.Controls.OfType<Button>()
                                 .OrderByDescending(b => b.Text == "Priority" ? 0 : 
                                                    b.Text == "View Issues" ? 1 :
                                                    b.Text == "Status" ? 2 :
                                                    b.Text == "Events" ? 3 :
                                                    b.Text == "Report" ? 4 : 5)
                                 .ToList();
            
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    int width = button.Text == "Priority" ? 90 : buttonWidth;
                    rightPosition -= width;
                    button.Location = new Point(rightPosition, topMargin);
                    button.Size = new Size(width, buttonHeight);
                    rightPosition -= buttonSpacing;
                }
            }
        }

        public void ShowReportForm()
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
                
                if (btnReport != null)
                {
                    ActivateButton(btnReport);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening report form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowReportedIssues()
        {
            try
            {
                // Create a panel to hold the issues
                var issuesPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.White,
                    Padding = new Padding(20)
                };

                // Clear the content panel and add the issues panel
                contentPanel.Controls.Clear();
                contentPanel.Controls.Add(issuesPanel);

                // Load the reported issues
                LoadReportedIssues(issuesPanel);

                // Update the active button
                var btnViewIssues = navigationPanel.Controls.OfType<Button>()
                    .FirstOrDefault(b => b.Text == "View Issues");
                    
                if (btnViewIssues != null)
                {
                    ActivateButton(btnViewIssues);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reported issues: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { 
                    BorderSize = 0,
                    MouseOverBackColor = Color.FromArgb(0, 40, 80) // Hover effect
                },
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                Height = 40,
                Width = text == "Priority" ? 90 : 100, // Adjust width based on text
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(5),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                TabStop = false // Remove focus rectangle
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

        private void ClearContentPanel()
        {
            contentPanel.Controls.Clear();
        }

        private void UpdateActiveButton(Button activeButton)
        {
            // Reset all buttons
            foreach (Control control in navigationPanel.Controls)
            {
                if (control is Button button)
                {
                    button.BackColor = Color.Transparent;
                    button.Font = new Font(button.Font, FontStyle.Regular);
                }
            }

            // Set active button style
            if (activeButton != null)
            {
                activeButton.BackColor = Color.FromArgb(0, 95, 179);
                activeButton.Font = new Font(activeButton.Font, FontStyle.Bold);
            }
        }

        private void ShowEventsForm()
        {
            try
            {
                ClearContentPanel();
                
                // Create a panel to hold the events form with padding
                var containerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var eventService = _serviceProvider.GetRequiredService<IEventService>();
                var recommendationService = _serviceProvider.GetRequiredService<IRecommendationService>();
                var eventsForm = new EventsForm(eventService, recommendationService);
                
                eventsForm.TopLevel = false;
                eventsForm.FormBorderStyle = FormBorderStyle.None;
                eventsForm.Dock = DockStyle.Fill;
                
                // Add the form to the container panel
                containerPanel.Controls.Add(eventsForm);
                
                // Add the container to the content panel
                contentPanel.Controls.Add(containerPanel);
                
                eventsForm.Show();
                UpdateActiveButton(btnEvents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowHomeForm()
        {
            try
            {
                contentPanel.Controls.Clear();
                var homeControl = new HomeForm(this);
                contentPanel.Controls.Add(homeControl);
                if (btnHome != null)
                {
                    ActivateButton(btnHome);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading home page: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowStatusForm()
        {
            try
            {
                Console.WriteLine("Creating service scope for ServiceRequestStatusForm...");
                contentPanel.Controls.Clear();
                
                // Create a container panel with padding
                var containerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };
                
                var statusForm = new ServiceRequestStatusForm
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                
                // Add the form to the container panel
                containerPanel.Controls.Add(statusForm);
                
                // Add the container to the content panel
                contentPanel.Controls.Add(containerPanel);
                
                statusForm.Show();
                UpdateActiveButton(btnStatus);
                
                // Force a layout update to ensure proper sizing
                contentPanel.PerformLayout();
                statusForm.PerformLayout();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error in ShowStatusForm: {ex.Message}\n\n{ex.StackTrace}";
                Console.WriteLine(errorMessage);
                MessageBox.Show($"Error opening service status: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Make sure the main form is shown even if there's an error
                this.Show();
                this.BringToFront();
            }
        }

        private void ShowPriorityQueueForm()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                using (var priorityQueueForm = scope.ServiceProvider.GetRequiredService<PriorityQueueForm>())
                {
                    priorityQueueForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening priority queue: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}