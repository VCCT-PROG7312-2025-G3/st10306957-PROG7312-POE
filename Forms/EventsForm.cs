using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROG7312_POE.Extensions;
using PROG7312_POE.Models;
using PROG7312_POE.Services;
using PlaceholderTextBox = PROG7312_POE.Extensions.PlaceholderTextBox;

namespace PROG7312_POE.Forms
{
    public partial class EventsForm : Form
    {
        // Services
        private readonly IEventService _eventService;
        private readonly IRecommendationService _recommendationService;
        
        // State
        private Event _selectedEvent;
        private bool _isLoading = false;
        
        // UI Controls
        private FlowLayoutPanel eventsPanel;
        private Panel loadingPanel;
        private Panel eventsContainer;
        private TableLayoutPanel mainLayout;
        
        private PlaceholderTextBox searchBox;
        private ComboBox categoryCombo;
        private List<Event> _allEvents = new List<Event>();
        
        // Constants
        private const int CardWidth = 280;
        private const int CardHeight = 200;
        private const int CardMargin = 15;

        public EventsForm(IEventService eventService, IRecommendationService recommendationService)
        {
            try
            {
                // Initialize services
                _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
                _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

                // Initialize form
                InitializeForm();
                
                // Load events when form is shown
                this.Shown += async (s, e) => await LoadEventsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void InitializeForm()
        {
            // Form properties
            this.Text = "Local Events and Announcements";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;
            this.Padding = new Padding(20);
            
            // Initialize main layout
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 80),  // Header
                    new RowStyle(SizeType.Absolute, 60),  // Filters
                    new RowStyle(SizeType.Percent, 100)   // Events list
                },
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            
            // Add header
            var headerLabel = new Label
            {
                Text = "Local Events",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var headerPanel = new Panel { Dock = DockStyle.Fill };
            headerPanel.Controls.Add(headerLabel);
            
            // Add filter panel
            var filterPanel = new Panel { Dock = DockStyle.Fill };
            
            // Add search box with placeholder text
            searchBox = new PlaceholderTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 10, 0),
                PlaceholderText = "Search events..."
            };
            searchBox.GotFocus += (s, e) => searchBox.Text = string.Empty;
            
            // Add category filter
            categoryCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            categoryCombo.Items.AddRange(new string[] { "All Categories", "Community", "Sports", "Education" });
            categoryCombo.SelectedIndex = 0;
            
            // Add filter layout
            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 60),
                    new ColumnStyle(SizeType.Percent, 30),
                    new ColumnStyle(SizeType.Absolute, 100)
                }
            };
            
            filterLayout.Controls.Add(searchBox, 0, 0);
            filterLayout.Controls.Add(categoryCombo, 1, 0);
            
            var clearButton = new Button
            {
                Text = "Clear Filters",
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                FlatStyle = FlatStyle.Flat
            };
            filterLayout.Controls.Add(clearButton, 2, 0);
            
            filterPanel.Controls.Add(filterLayout);
            
            // Initialize events container
            eventsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            
            // Initialize events panel
            eventsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10)
            };
            
            // Initialize loading panel
            var loadingLabel = new Label
            {
                Text = "Loading events...",
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            
            loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };
            loadingPanel.Controls.Add(loadingLabel);
            
            // Add panels to container
            eventsContainer.Controls.Add(eventsPanel);
            eventsContainer.Controls.Add(loadingPanel);
            
            // Add controls to main layout
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(filterPanel, 0, 1);
            mainLayout.Controls.Add(eventsContainer, 0, 2);
            
            // Add main layout to form
            this.Controls.Add(mainLayout);
            
            // Wire up events
            searchBox.TextChanged += async (s, e) => await FilterEventsAsync();
            categoryCombo.SelectedIndexChanged += async (s, e) => await FilterEventsAsync();
            clearButton.Click += async (s, e) => await ClearFiltersAsync();
        }
        
        private async Task LoadEventsAsync()
        {
            if (_isLoading) return;
            
            try
            {
                _isLoading = true;
                ShowLoading(true);
                
                // Load events from service and store them
                _allEvents = (await _eventService.GetUpcomingEventsAsync())?.ToList() ?? new List<Event>();
                UpdateEventsList(_allEvents);
                
                // Load recommendations if available
                var recommendations = await _recommendationService?.GetRecommendedEventsAsync();
                if (recommendations?.Any() == true)
                {
                    await ShowRecommendedEventsAsync(recommendations);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                ShowLoading(false);
            }
        }
        
        private void UpdateEventsList(IEnumerable<Event> events)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateEventsList(events)));
                return;
            }
            
            eventsPanel.SuspendLayout();
            eventsPanel.Controls.Clear();
            
            var eventsList = events?.ToList() ?? new List<Event>();
            
            if (!eventsList.Any())
            {
                var noResultsLabel = new Label
                {
                    Text = "No events found. Try adjusting your search criteria.",
                    Font = new Font("Segoe UI", 12, FontStyle.Italic),
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true
                };
                eventsPanel.Controls.Add(noResultsLabel);
            }
            else
            {
                foreach (var evt in eventsList)
                {
                    var card = CreateEventCard(evt);
                    eventsPanel.Controls.Add(card);
                }
            }
            
            eventsPanel.ResumeLayout();
        }
        
        private Control CreateEventCard(Event evt)
        {
            var card = new Panel
            {
                Width = CardWidth,
                Height = CardHeight,
                Margin = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Padding = new Padding(0)
            };

            // Get category with null check
            var category = evt.Category ?? "General";
            
            // Card header with title and category
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = GetCategoryColor(category),
                Padding = new Padding(10, 5, 10, 5)
            };

            var titleLabel = new Label
            {
                Text = evt.Title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var categoryLabel = new Label
            {
                Text = category,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.White,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 15
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(categoryLabel);

            // Card body with event details
            var bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 5, 10, 5)
            };

            // Handle event date and time
            var dateTimeText = $"📅 {evt.StartDate:dd MMM yyyy 'at' HH:mm}";
            if (evt.EndDate.HasValue)
            {
                dateTimeText += $" - {evt.EndDate:HH:mm}";
            }

            var dateLabel = new Label
            {
                Text = dateTimeText,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 5)
            };

            var locationLabel = new Label
            {
                Text = $"📍 {evt.Location ?? "Location not specified"}",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 5)
            };

            var descriptionText = !string.IsNullOrEmpty(evt.Description) 
                ? (evt.Description.Length > 100 ? evt.Description.Substring(0, 100) + "..." : evt.Description)
                : "No description available";

            var descriptionLabel = new Label
            {
                Text = descriptionText,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 5, 0, 0)
            };

            // Add controls to body panel
            bodyPanel.Controls.Add(descriptionLabel);
            bodyPanel.Controls.Add(locationLabel);
            bodyPanel.Controls.Add(dateLabel);

            // Add panels to card
            card.Controls.Add(bodyPanel);
            card.Controls.Add(headerPanel);
            
            // Add click event to show details
            card.Click += (s, e) => ShowEventDetails(evt);
            
            return card;
        }

        private Color GetCategoryColor(string category)
        {
            if (string.IsNullOrEmpty(category))
                return Color.FromArgb(70, 130, 180); // Default color

            switch (category.ToLower())
            {
                case "community":
                    return Color.FromArgb(65, 105, 225);  // Royal Blue
                case "sports":
                    return Color.FromArgb(34, 139, 34);   // Forest Green
                case "education":
                    return Color.FromArgb(220, 20, 60);   // Crimson Red
                default:
                    return Color.FromArgb(70, 130, 180);  // Steel Blue (default)
            }
        }
        
        
        private void ShowEventDetails(Event evt)
        {
            _selectedEvent = evt;
            
            // Create main container with padding
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(25, 20, 25, 20),
                BackColor = Color.White
            };
            
            var detailsForm = new Form
            {
                Text = evt.Title,
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            var detailsText = new TextBox
            {
                Text = $"Title: {evt.Title}\r\n\r\nDescription: {evt.Description}",
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20)
            };
            
            detailsForm.Controls.Add(detailsText);
            detailsForm.ShowDialog(this);
        }
        
        private async Task FilterEventsAsync()
        {
            try
            {
                if (searchBox == null || categoryCombo == null) return;

                var searchText = searchBox.Text.Trim().ToLower();
                var selectedCategory = categoryCombo.SelectedItem?.ToString() ?? "All Categories";

                // Get all events if not already loaded
                if (!_allEvents.Any())
                {
                    _allEvents = (await _eventService.GetUpcomingEventsAsync())?.ToList() ?? new List<Event>();
                }

                // Apply filters
                var filteredEvents = _allEvents.AsEnumerable();

                // Filter by search text
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredEvents = filteredEvents.Where(e =>
                        e.Title.ToLower().Contains(searchText) ||
                        e.Description.ToLower().Contains(searchText));
                }

                // Filter by category
                if (selectedCategory != "All Categories")
                {
                    filteredEvents = filteredEvents.Where(e => 
                        string.Equals(e.Category, selectedCategory, StringComparison.OrdinalIgnoreCase));
                }

                // Update UI
                UpdateEventsList(filteredEvents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering events: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task ClearFiltersAsync()
        {
            try
            {
                if (searchBox != null) searchBox.Text = string.Empty;
                if (categoryCombo != null && categoryCombo.Items.Count > 0) 
                    categoryCombo.SelectedIndex = 0;
                
                await LoadEventsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing filters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task ShowRecommendedEventsAsync(IEnumerable<Event> recommendations)
        {
            // Implementation for showing recommended events
            await Task.Delay(100); // Simulate async operation
        }
        
        private void ShowLoading(bool show)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowLoading(show)));
                return;
            }
            
            eventsPanel.Visible = !show;
            loadingPanel.Visible = show;
            loadingPanel.BringToFront();
        }
    }
}
