using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PROG7312_POE.Models;
using PROG7312_POE.Services;

namespace PROG7312_POE.Forms
{
    public partial class EventsForm : Form
    {
        private readonly IEventService _eventService;
        private Event _selectedEvent;
        private bool _isLoading = true;

        public EventsForm(IEventService eventService)
        {
            InitializeComponent();
            _eventService = eventService;
            
            // Configure form
            this.Text = "Local Events and Announcements";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(1000, 700);
            this.BackColor = Color.White;
            this.Padding = new Padding(20);
            
            InitializeComponents();
            _ = LoadEventsAsync();
        }

        private void InitializeComponents()
        {
            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = 
                {
                    new RowStyle(SizeType.Absolute, 40),  // Header
                    new RowStyle(SizeType.Absolute, 40),  // Filters
                    new RowStyle(SizeType.Percent, 100)   // Events list
                },
                Padding = new Padding(0, 0, 0, 10)
            };

            // Header
            var headerLabel = new Label
            {
                Text = "Local Events and Announcements",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 64, 122),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            // Filter controls
            var filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Search box with placeholder text implementation
            var searchBox = new TextBox
            {
                Text = "Search events...",
                ForeColor = SystemColors.GrayText,
                Width = 300,
                Location = new Point(0, 5)
            };
            
            searchBox.Enter += (s, e) => 
            {
                if (searchBox.Text == "Search events...")
                {
                    searchBox.Text = "";
                    searchBox.ForeColor = SystemColors.WindowText;
                }
            };
            
            searchBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "Search events...";
                    searchBox.ForeColor = SystemColors.GrayText;
                }
            };
            
            searchBox.TextChanged += (s, e) => 
            {
                if (searchBox.Text != "Search events...")
                {
                    FilterEventsAsync();
                }
            };

            var categoryCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Location = new Point(320, 5)
            };
            categoryCombo.SelectedIndexChanged += (s, e) => FilterEventsAsync();

            var dateFilterCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180,
                Location = new Point(530, 5)
            };
            dateFilterCombo.Items.AddRange(new object[] { "All Dates", "Today", "This Week", "This Month", "Next 30 Days" });
            dateFilterCombo.SelectedIndex = 0;
            dateFilterCombo.SelectedIndexChanged += async (s, e) => await FilterEventsAsync();

            var addButton = new Button
            {
                Text = "+ Add Event",
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 30,
                Location = new Point(720, 5)
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => ShowAddEditEventForm();

            filterPanel.Controls.Add(searchBox);
            filterPanel.Controls.Add(categoryCombo);
            filterPanel.Controls.Add(dateFilterCombo);
            filterPanel.Controls.Add(addButton);

            // Events list
            var eventsPanel = new FlowLayoutPanel
            {
                Name = "eventsPanel",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScrollMargin = new Size(0, 10),
                Padding = new Padding(5)
            };

            // Store references to controls we'll need later using Tag property
            searchBox.Tag = "searchBox";
            categoryCombo.Tag = "categoryCombo";
            dateFilterCombo.Tag = "dateFilterCombo";
            addButton.Tag = "addButton";
            
            // Add controls to filter panel
            filterPanel.Controls.Add(searchBox);
            filterPanel.Controls.Add(categoryCombo);
            filterPanel.Controls.Add(dateFilterCombo);
            filterPanel.Controls.Add(addButton);

            // Add controls to main layout
            mainLayout.Controls.Add(headerLabel, 0, 0);
            mainLayout.Controls.Add(filterPanel, 0, 1);
            mainLayout.Controls.Add(eventsPanel, 0, 2);
            
            // Add main layout to form
            this.Controls.Add(mainLayout);
        }

        private async Task LoadEventsAsync()
        {
            try
            {
                _isLoading = true;
                Console.WriteLine("Loading events...");
                var events = (await _eventService.GetUpcomingEventsAsync()).ToList();
                Console.WriteLine($"Found {events.Count} events");
                
                var eventsPanel = this.Controls.OfType<TableLayoutPanel>()
                    .FirstOrDefault()
                    ?.Controls.OfType<FlowLayoutPanel>()
                    .FirstOrDefault(p => p.Name == "eventsPanel");
                
                Console.WriteLine($"eventsPanel found: {eventsPanel != null}");
                
                if (eventsPanel != null)
                {
                    Console.WriteLine("Updating events list...");
                    await UpdateEventsList(events);
                    await UpdateCategoryFilter();
                }
                else
                {
                    Console.WriteLine("ERROR: Could not find eventsPanel in form hierarchy");
                    MessageBox.Show("Error: Could not initialize events display. Please restart the application.", 
                        "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading events: {ex.Message}\n\n{ex.StackTrace}";
                Console.WriteLine(errorMsg);
                MessageBox.Show($"Error loading events: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task UpdateEventsList(IEnumerable<Event> events)
        {
            // Make sure we're on the UI thread
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { _ = UpdateEventsList(events); });
                return;
            }

            try
            {
                Console.WriteLine("Finding events panel...");
                var mainTable = this.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                var eventsPanel = mainTable?.Controls.OfType<FlowLayoutPanel>()
                    .FirstOrDefault(p => p.Name == "eventsPanel");

                if (eventsPanel == null)
                {
                    Console.WriteLine("ERROR: Could not find events panel in UpdateEventsList");
                    return;
                }

                Console.WriteLine($"Updating events list with {events.Count()} events");

                // Clear existing events
                eventsPanel.SuspendLayout();
                eventsPanel.Controls.Clear();

                var eventList = events.ToList();
                if (!eventList.Any())
                {
                    var noEventsLabel = new Label
                    {
                        Text = "No events found. Try adjusting your filters or add a new event.",
                        Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        AutoSize = true,
                        Margin = new Padding(0, 20, 0, 0)
                    };
                    eventsPanel.Controls.Add(noEventsLabel);
                }
                else
                {
                    foreach (var evt in eventList.OrderBy(e => e.StartDate))
                    {
                        Console.WriteLine($"Creating card for event: {evt.Title}");
                        var eventCard = CreateEventCard(evt);
                        eventsPanel.Controls.Add(eventCard);
                    }
                }

                eventsPanel.ResumeLayout(true);
                Console.WriteLine("Finished updating events list");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UpdateEventsList: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private Panel CreateEventCard(Event evt)
        {
            var card = new Panel
            {
                Width = 900,
                Height = 120,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(15),
                Cursor = Cursors.Hand,
                Tag = evt.Id
            };

            // Add hover effect
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(245, 245, 245);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
            card.Click += (s, e) => ShowEventDetails(evt);

            // Create layout for the card
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

            // Event title
            var titleLabel = new Label
            {
                Text = evt.Title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(0, 64, 122)
            };

            // Date and time
            var dateText = evt.StartDate.ToString("f");
            if (evt.EndDate.HasValue)
            {
                dateText += $" - {evt.EndDate.Value.ToString("t")}";
            }
            var dateLabel = new Label
            {
                Text = dateText,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            // Category badge
            var categoryLabel = new Label
            {
                Text = evt.Category,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = GetCategoryColor(evt.Category),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 2, 5, 2),
                Height = 20
            };

            // Location
            var locationPanel = new Panel { Dock = DockStyle.Fill };
            var locationLabel = new Label
            {
                Text = $"📍 {evt.Location}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                Location = new Point(0, 5)
            };
            locationPanel.Controls.Add(locationLabel);

            // Attendees
            var attendeesLabel = new Label
            {
                Text = $"👥 {evt.CurrentAttendees} / {evt.MaxAttendees}",
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray
            };

            // Organizer
            var organizerLabel = new Label
            {
                Text = $"By: {evt.Organizer}",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray
            };

            // Add controls to layout
            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(dateLabel, 1, 0);
            layout.Controls.Add(categoryLabel, 2, 0);
            layout.Controls.Add(locationPanel, 0, 1);
            layout.Controls.Add(organizerLabel, 1, 1);
            layout.Controls.Add(attendeesLabel, 2, 1);

            // Add layout to card
            card.Controls.Add(layout);

            return card;
        }

        private Color GetCategoryColor(string category)
        {
            // Simple hash function to generate consistent colors for categories
            int hash = category.GetHashCode();
            return Color.FromArgb(
                (hash & 0xFF0000) >> 16,
                (hash & 0x00FF00) >> 8,
                (hash & 0x0000FF)
            );
        }

        private async Task UpdateCategoryFilter()
        {
            try
            {
                // Find the main table layout panel
                var mainTable = this.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                if (mainTable == null) return;

                // Find the filter panel (second row of the main table)
                var filterPanel = mainTable.GetControlFromPosition(0, 1) as Panel;
                if (filterPanel == null) return;

                // Find the category combo box by its tag
                var categoryCombo = filterPanel.Controls.OfType<ComboBox>()
                    .FirstOrDefault(c => c.Tag?.ToString() == "categoryCombo");

                if (categoryCombo == null)
                {
                    Console.WriteLine("Category combo box not found in UpdateCategoryFilter");
                    return;
                }

                Console.WriteLine("Updating category filter...");
                var categories = (await _eventService.GetEventCategoriesAsync()).ToList();
                Console.WriteLine($"Found {categories.Count} categories");

                // Store the current selection if there is one
                var currentSelection = categoryCombo.SelectedItem?.ToString();

                // Update the combo box items
                categoryCombo.BeginUpdate();
                try
                {
                    categoryCombo.Items.Clear();
                    categoryCombo.Items.Add("All Categories");
                    categoryCombo.Items.AddRange(categories.ToArray());

                    // Try to restore the previous selection if it still exists
                    if (!string.IsNullOrEmpty(currentSelection) && categories.Contains(currentSelection))
                    {
                        categoryCombo.SelectedItem = currentSelection;
                    }
                    else
                    {
                        categoryCombo.SelectedIndex = 0;
                    }
                    
                    Console.WriteLine($"Category filter updated. Selected: {categoryCombo.SelectedItem}");
                }
                finally
                {
                    categoryCombo.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCategoryFilter: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task FilterEventsAsync()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;

                // Get references to the filter controls
                var mainTable = this.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                if (mainTable == null) return;

                var filterPanel = mainTable.GetControlFromPosition(0, 1) as Panel;
                if (filterPanel == null) return;

                var searchBox = filterPanel.Controls.OfType<TextBox>().FirstOrDefault();
                var categoryCombo = filterPanel.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Tag?.ToString() == "categoryCombo");
                var dateFilterCombo = filterPanel.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Tag?.ToString() == "dateFilterCombo");

                if (searchBox == null || categoryCombo == null || dateFilterCombo == null) return;

                Console.WriteLine($"Filtering - Search: '{searchBox.Text}', Category: {categoryCombo.SelectedItem}, Date: {dateFilterCombo.SelectedItem}");

                // Get base events based on search
                IEnumerable<Event> events;
                if (string.IsNullOrWhiteSpace(searchBox.Text) || searchBox.Text == "Search events...")
                {
                    events = await _eventService.GetAllEventsAsync();
                }
                else
                {
                    events = await _eventService.SearchEventsAsync(searchBox.Text);
                }

                var eventList = events.ToList();
                Console.WriteLine($"After search: {eventList.Count} events");

                // Apply category filter
                if (categoryCombo.SelectedIndex > 0 && categoryCombo.SelectedItem != null)
                {
                    var category = categoryCombo.SelectedItem.ToString();
                    Console.WriteLine($"Filtering by category: {category}");
                    var categoryEvents = await _eventService.GetEventsByCategoryAsync(category);
                    eventList = eventList.Intersect(categoryEvents).ToList();
                    Console.WriteLine($"After category filter: {eventList.Count} events");
                }

                // Apply date filter
                var now = DateTime.Now;
                var selectedDateFilter = dateFilterCombo.SelectedItem?.ToString();
                Console.WriteLine($"Applying date filter: {selectedDateFilter}");
                
                switch (selectedDateFilter)
                {
                    case "Today":
                        eventList = eventList.Where(e => e.StartDate.Date == now.Date).ToList();
                        break;
                    case "This Week":
                        var endOfWeek = now.Date.AddDays(7 - (int)now.DayOfWeek);
                        eventList = eventList.Where(e => e.StartDate.Date >= now.Date && e.StartDate.Date <= endOfWeek).ToList();
                        break;
                    case "This Month":
                        var endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                        eventList = eventList.Where(e => e.StartDate.Date >= now.Date && e.StartDate.Date <= endOfMonth).ToList();
                        break;
                    case "Next 30 Days":
                        var in30Days = now.Date.AddDays(30);
                        eventList = eventList.Where(e => e.StartDate.Date >= now.Date && e.StartDate.Date <= in30Days).ToList();
                        break;
                    // "All Dates" - no filter
                }

                Console.WriteLine($"Final event count: {eventList.Count}");
                await UpdateEventsList(eventList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FilterEventsAsync: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error filtering events: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ShowEventDetails(Event evt)
        {
            _selectedEvent = evt;
            
            var detailsForm = new Form
            {
                Text = "Event Details",
                Size = new Size(600, 700),
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
                RowCount = 10,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 120),
                    new ColumnStyle(SizeType.Percent, 100)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 40),  // Title
                    new RowStyle(SizeType.Absolute, 30),  // Date
                    new RowStyle(SizeType.Absolute, 30),  // Location
                    new RowStyle(SizeType.Absolute, 30),  // Organizer
                    new RowStyle(SizeType.Absolute, 30),  // Category
                    new RowStyle(SizeType.Absolute, 30),  // Attendees
                    new RowStyle(SizeType.Absolute, 30),  // Contact
                    new RowStyle(SizeType.Absolute, 30),  // Price
                    new RowStyle(SizeType.Percent, 100),  // Description
                    new RowStyle(SizeType.Absolute, 50)   // Buttons
                }
            };

            // Title
            var titleLabel = new Label
            {
                Text = evt.Title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 64, 122),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            layout.SetColumnSpan(titleLabel, 2);
            layout.Controls.Add(titleLabel, 0, 0);

            // Date
            var dateText = $"📅 {evt.StartDate:f}";
            if (evt.EndDate.HasValue)
            {
                dateText += $" to {evt.EndDate.Value:f}";
            }
            var dateLabel = new Label
            {
                Text = dateText,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(dateLabel, 2);
            layout.Controls.Add(dateLabel, 0, 1);

            // Location
            var locationLabel = new Label
            {
                Text = $"📍 {evt.Location}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(locationLabel, 2);
            layout.Controls.Add(locationLabel, 0, 2);

            // Organizer
            var organizerLabel = new Label
            {
                Text = $"👤 {evt.Organizer}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(organizerLabel, 2);
            layout.Controls.Add(organizerLabel, 0, 3);

            // Category
            var categoryLabel = new Label
            {
                Text = $"🏷️ {evt.Category}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(categoryLabel, 2);
            layout.Controls.Add(categoryLabel, 0, 4);

            // Attendees
            var attendeesLabel = new Label
            {
                Text = $"👥 {evt.CurrentAttendees} / {evt.MaxAttendees} attendees",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(attendeesLabel, 2);
            layout.Controls.Add(attendeesLabel, 0, 5);

            // Contact
            var contactLabel = new Label
            {
                Text = $"📧 {evt.ContactEmail} | 📞 {evt.ContactPhone}",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(contactLabel, 2);
            layout.Controls.Add(contactLabel, 0, 6);

            // Price
            var priceText = evt.Price.HasValue && evt.Price > 0 
                ? $"💵 ${evt.Price:N2}" 
                : "💵 Free";
            var priceLabel = new Label
            {
                Text = priceText,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            layout.SetColumnSpan(priceLabel, 2);
            layout.Controls.Add(priceLabel, 0, 7);

            // Description
            var descLabel = new TextBox
            {
                Text = evt.Description,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 10)
            };
            layout.SetColumnSpan(descLabel, 2);
            layout.Controls.Add(descLabel, 0, 8);

            // Buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            var closeButton = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(0, 64, 122),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => detailsForm.Close();

            var registerButton = new Button
            {
                Text = "Register",
                BackColor = Color.FromArgb(0, 122, 61),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 100,
                Height = 35,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(0, 0, 110, 0)
            };
            registerButton.FlatAppearance.BorderSize = 0;
            registerButton.Click += async (s, e) => await RegisterForEventAsync(evt.Id);

            buttonPanel.Controls.Add(registerButton);
            buttonPanel.Controls.Add(closeButton);

            layout.Controls.Add(buttonPanel, 0, 9);
            layout.SetColumnSpan(buttonPanel, 2);

            detailsForm.Controls.Add(layout);
            detailsForm.ShowDialog(this);
        }

        private async Task RegisterForEventAsync(int eventId)
        {
            try
            {
                var result = await _eventService.RegisterForEventAsync(eventId, "Current User", "user@example.com");
                if (result)
                {
                    MessageBox.Show("Successfully registered for the event!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadEventsAsync();
                }
                else
                {
                    MessageBox.Show("Could not register for the event. It may be full.", "Registration Failed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering for event: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAddEditEventForm(Event eventToEdit = null)
        {
            // TODO: Implement add/edit event form
            MessageBox.Show("Add/Edit Event functionality will be implemented here.", "Coming Soon", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
