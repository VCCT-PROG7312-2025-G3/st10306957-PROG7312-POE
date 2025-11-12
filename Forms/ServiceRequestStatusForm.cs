using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PROG7312_POE.DataStructures;
using PROG7312_POE.Models;
using PROG7312_POE.Extensions;
using System.ComponentModel;

namespace PROG7312_POE.Forms
{
    public partial class ServiceRequestStatusForm : Form
    {
        private readonly ServiceRequestTree _requestTree;
        private readonly ServiceRequestGraph _requestGraph;
        private List<ServiceRequest> _filteredRequests;
        private readonly Random _random = new Random();

        // UI Controls
        private TextBox txtSearch;
        private ComboBox cmbStatusFilter;
        private ComboBox cmbPriorityFilter;
        private DataGridView dgvRequests;
        private Button btnSearch;
        private Button btnResetFilters;
        private Panel pnlRequestDetails;
        private Label lblRequestDetails;
        private ProgressBar progressBar1;
        
        public ServiceRequestStatusForm()
        {
            _requestTree = new ServiceRequestTree();
            _requestGraph = new ServiceRequestGraph();
            _filteredRequests = new List<ServiceRequest>();

            InitializeComponentCustom();
            LoadSampleData();
            UpdateRequestList();
        }

        private void InitializeComponentCustom()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "Service Request Status";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(15);
            this.BackColor = Color.White;
            this.MinimumSize = new Size(800, 600); // Set minimum size to prevent controls from overlapping

            // Search box
            txtSearch = new PlaceholderTextBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 30),
                PlaceholderText = "Search..."
            };
            txtSearch.TextChanged += (s, e) => FilterRequests();

            // Status filter
            cmbStatusFilter = new ComboBox
            {
                Location = new Point(330, 15),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbStatusFilter.Items.AddRange(Enum.GetNames(typeof(RequestStatus)));
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterRequests();
            
            // Priority filter
            cmbPriorityFilter = new ComboBox
            {
                Location = new Point(490, 15),
                Size = new Size(140, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbPriorityFilter.Items.Add("All Priorities");
            cmbPriorityFilter.Items.AddRange(Enum.GetNames(typeof(RequestPriority)));
            cmbPriorityFilter.SelectedIndex = 0;
            cmbPriorityFilter.SelectedIndexChanged += (s, e) => FilterRequests();

            // Search button
            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(640, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => FilterRequests();

            // Reset filters button
            btnResetFilters = new Button
            {
                Text = "Reset",
                Location = new Point(750, 15),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnResetFilters.Click += (s, e) =>
            {
                txtSearch.Text = string.Empty;
                cmbStatusFilter.SelectedIndex = -1;
                cmbPriorityFilter.SelectedIndex = 0;
                FilterRequests();
            };

            // DataGridView for requests
            dgvRequests = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(800, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(240, 240, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(5)
            };
            dgvRequests.SelectionChanged += DgvRequests_SelectionChanged;

            // Details panel
            pnlRequestDetails = new Panel
            {
                Location = new Point(830, 60),
                Size = new Size(400, 500),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Progress bar
            progressBar1 = new ProgressBar
            {
                Location = new Point(15, 570),
                Size = new Size(800, 20),
                Style = ProgressBarStyle.Continuous,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(5, 10, 5, 10)
            };

            // Main container panel
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                ColumnStyles = 
                {
                    new ColumnStyle(SizeType.Percent, 70F) { Width = 70 },
                    new ColumnStyle(SizeType.Percent, 30F) { Width = 30 }
                },
                RowStyles = 
                {
                    new RowStyle(SizeType.Absolute, 60F),  // Search/filter row
                    new RowStyle(SizeType.Percent, 100F),  // Content row
                    new RowStyle(SizeType.Absolute, 60F)   // Progress bar row
                },
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.White
            };

            // Search panel (top row, spans both columns)
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 15, 10, 10),
                BackColor = Color.White
            };
            
            // Set fixed sizes for controls
            txtSearch.Width = 200;
            cmbStatusFilter.Width = 150;
            cmbPriorityFilter.Width = 120;
            btnSearch.Width = 80;
            btnResetFilters.Width = 80;
            
            // Add spacing between controls
            searchPanel.Controls.Add(txtSearch);
            searchPanel.SetFlowBreak(txtSearch, false);
            
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.SetFlowBreak(cmbStatusFilter, false);
            
            searchPanel.Controls.Add(cmbPriorityFilter);
            searchPanel.SetFlowBreak(cmbPriorityFilter, false);
            
            searchPanel.Controls.Add(btnSearch);
            searchPanel.SetFlowBreak(btnSearch, false);
            
            searchPanel.Controls.Add(btnResetFilters);
            searchPanel.SetFlowBreak(btnResetFilters, true);
            
            // Data grid view (left side, middle row)
            dgvRequests.Dock = DockStyle.Fill;
            dgvRequests.Margin = new Padding(5);
            
            // Details panel (right side, middle row)
            pnlRequestDetails.Dock = DockStyle.Fill;
            
            // Progress bar (bottom row, spans both columns)
            progressBar1.Dock = DockStyle.Top;
            progressBar1.Margin = new Padding(10, 5, 10, 5);
            progressBar1.Height = 30;
            
            var progressContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BackColor = Color.White
            };
            progressContainer.Controls.Add(progressBar1);
            
            // Add all controls to the main container
            mainContainer.Controls.Add(searchPanel, 0, 0);
            mainContainer.SetColumnSpan(searchPanel, 2);
            
            mainContainer.Controls.Add(dgvRequests, 0, 1);
            mainContainer.Controls.Add(pnlRequestDetails, 1, 1);
            
            mainContainer.Controls.Add(progressContainer, 0, 2);
            mainContainer.SetColumnSpan(progressContainer, 2);
            
            // Add the main container to the form
            this.Controls.Add(mainContainer);
            
            // No need for manual positioning with FlowLayoutPanel

            // Setup data grid view columns
            SetupDataGridView();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void UpdateLayout()
        {
            // This method is no longer needed as we're using Dock and Anchor properties
            // for layout management
        }

        private void SetupDataGridView()
        {
            dgvRequests.Columns.Clear();

            // Explicitly type the array to avoid implicit typing issues
            DataGridViewColumn[] columns = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "RequestId", HeaderText = "Request ID", DataPropertyName = "RequestId", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Title", HeaderText = "Title", DataPropertyName = "Title", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Priority", HeaderText = "Priority", DataPropertyName = "Priority", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "DateSubmitted", HeaderText = "Submitted", DataPropertyName = "DateSubmitted", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "AssignedDepartment", HeaderText = "Department", DataPropertyName = "AssignedDepartment", Width = 120 },
                new DataGridViewProgressColumn { Name = "Progress", HeaderText = "Progress", DataPropertyName = "ProgressPercentage", Width = 100 }
            };

            dgvRequests.Columns.AddRange(columns);
        }


        private void LoadSampleData()
        {
            var departments = new[] { "IT", "Facilities", "HR", "Finance", "Operations" };
            var statuses = Enum.GetValues(typeof(RequestStatus)).Cast<RequestStatus>().ToArray();
            var priorities = Enum.GetValues(typeof(RequestPriority)).Cast<RequestPriority>().ToArray();

            for (int i = 0; i < 20; i++)
            {
                var request = new ServiceRequest
                {
                    Title = $"Sample Request {i + 1}",
                    Description = $"This is a sample service request description for request {i + 1}.",
                    Status = statuses[_random.Next(statuses.Length)],
                    Priority = priorities[_random.Next(priorities.Length)],
                    AssignedDepartment = departments[_random.Next(departments.Length)],
                    ProgressPercentage = _random.Next(0, 101),
                    Location = $"Building {_random.Next(1, 10)}-{_random.Next(100, 500)}",
                    SubmittedBy = $"User{_random.Next(1, 50)}@example.com",
                    ContactEmail = $"user{_random.Next(1, 50)}@example.com",
                    ContactPhone = $"({_random.Next(100, 999)}) {_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                    Notes = _random.Next(3) == 0 ? "Urgent attention required" : ""
                };

                if (request.Status == RequestStatus.Completed)
                {
                    request.ActualCompletionDate = DateTime.Now.AddDays(-_random.Next(1, 30));
                }

                _requestTree.Insert(request);
                _requestGraph.AddRequest(request);

                // Add some dependencies
                if (i > 0 && _random.Next(3) == 0) // 1 in 3 chance to add a dependency
                {
                    var previousRequest = _requestTree.Find($"SR-{DateTime.Now:yyyyMMdd}-{i:D6}");
                    if (previousRequest != null)
                    {
                        _requestGraph.AddDependency(request.RequestId, previousRequest.RequestId);
                    }
                }
            }
        }

        private void FilterRequests()
        {
            var searchText = txtSearch.Text.ToLower();
            var statusFilter = cmbStatusFilter.SelectedItem?.ToString();
            var priorityFilter = cmbPriorityFilter.SelectedItem?.ToString();

            _filteredRequests = _requestTree.GetAll()
                .Where(r => string.IsNullOrEmpty(searchText) ||
                           r.RequestId.ToLower().Contains(searchText) ||
                           r.Title.ToLower().Contains(searchText) ||
                           r.Description.ToLower().Contains(searchText))
                .Where(r => string.IsNullOrEmpty(statusFilter) || r.Status.ToString() == statusFilter)
                .Where(r => priorityFilter == "All Priorities" || string.IsNullOrEmpty(priorityFilter) || r.Priority.ToString() == priorityFilter)
                .OrderByDescending(r => r.DateSubmitted)
                .ToList();

            UpdateRequestList();
        }

        private void UpdateRequestList()
        {
            dgvRequests.DataSource = null;
            dgvRequests.DataSource = _filteredRequests;
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (_filteredRequests.Count == 0)
            {
                progressBar1.Visible = false;
                return;
            }

            progressBar1.Visible = true;
            progressBar1.Value = (int)_filteredRequests.Average(r => r.ProgressPercentage);
        }

        private void DgvRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;

            var selectedRequest = dgvRequests.SelectedRows[0].DataBoundItem as ServiceRequest;
            if (selectedRequest != null)
            {
                ShowRequestDetails(selectedRequest);
            }
        }

        private void ShowRequestDetails(ServiceRequest request)
        {
            pnlRequestDetails.Controls.Clear();

            var y = 10;
            var x = 10;
            var width = pnlRequestDetails.Width - 40;

            // Title
            var lblTitle = new Label
            {
                Text = request.Title,
                Location = new Point(x, y),
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(width, 0)
            };
            pnlRequestDetails.Controls.Add(lblTitle);
            y += lblTitle.Height + 10;

            // Status and Priority
            var statusColor = GetStatusColor(request.Status);
            var lblStatus = new Label
            {
                Text = $"Status: {request.Status}",
                Location = new Point(x, y),
                ForeColor = statusColor,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                AutoSize = true
            };
            pnlRequestDetails.Controls.Add(lblStatus);

            var priorityColor = GetPriorityColor(request.Priority);
            var lblPriority = new Label
            {
                Text = $"Priority: {request.Priority}",
                Location = new Point(x + 150, y),
                ForeColor = priorityColor,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                AutoSize = true
            };
            pnlRequestDetails.Controls.Add(lblPriority);
            y += 30;

            // Progress bar
            var progressBar = new ProgressBar
            {
                Location = new Point(x, y),
                Size = new Size(width, 20),
                Value = request.ProgressPercentage
            };
            pnlRequestDetails.Controls.Add(progressBar);
            y += 30;

            // Progress percentage
            var lblProgress = new Label
            {
                Text = $"Progress: {request.ProgressPercentage}%",
                Location = new Point(x, y),
                AutoSize = true
            };
            pnlRequestDetails.Controls.Add(lblProgress);
            y += 25;

            // Details section
            AddDetailSection("Description", request.Description, ref x, ref y, width);
            AddDetailSection("Location", request.Location, ref x, ref y, width);
            AddDetailSection("Submitted By", request.SubmittedBy, ref x, ref y, width);
            AddDetailSection("Assigned Department", request.AssignedDepartment, ref x, ref y, width);
            AddDetailSection("Contact Email", request.ContactEmail, ref x, ref y, width);
            AddDetailSection("Contact Phone", request.ContactPhone, ref x, ref y, width);

            if (request.EstimatedCompletionDate.HasValue)
            {
                AddDetailSection("Estimated Completion", request.EstimatedCompletionDate.Value.ToString("f"), ref x, ref y, width);
            }

            if (request.ActualCompletionDate.HasValue)
            {
                AddDetailSection("Completed On", request.ActualCompletionDate.Value.ToString("f"), ref x, ref y, width);
            }

            // Status History
            if (request.StatusHistory != null && request.StatusHistory.Count > 0)
            {
                y += 10;
                var lblHistory = new Label
                {
                    Text = "Status History:",
                    Location = new Point(x, y),
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    AutoSize = true
                };
                pnlRequestDetails.Controls.Add(lblHistory);
                y += 25;

                foreach (var history in request.StatusHistory.OrderByDescending(h => h.Timestamp).Take(5))
                {
                    var historyText = $"{history.Timestamp:g} - {history.Status}";
                    if (!string.IsNullOrEmpty(history.Notes))
                    {
                        historyText += $" ({history.Notes})";
                    }

                    var lblHistoryItem = new Label
                    {
                        Text = historyText,
                        Location = new Point(x + 10, y),
                        AutoSize = true,
                        MaximumSize = new Size(width - 20, 0)
                    };
                    pnlRequestDetails.Controls.Add(lblHistoryItem);
                    y += lblHistoryItem.Height + 5;
                }
            }

            // Related Requests
            var relatedRequests = _requestGraph.GetAllRelatedRequests(request.RequestId);
            if (relatedRequests.Any())
            {
                y += 10;
                var lblRelated = new Label
                {
                    Text = "Related Requests:",
                    Location = new Point(x, y),
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    AutoSize = true
                };
                pnlRequestDetails.Controls.Add(lblRelated);
                y += 25;

                foreach (var relatedRequest in relatedRequests.Take(3))
                {
                    var btnRelated = new Button
                    {
                        Text = $"{relatedRequest.RequestId} - {relatedRequest.Title}",
                        Location = new Point(x + 10, y),
                        Size = new Size(width - 20, 25),
                        TextAlign = ContentAlignment.MiddleLeft,
                        FlatStyle = FlatStyle.Flat,
                        Tag = relatedRequest.RequestId
                    };
                    btnRelated.Click += (s, e) =>
                    {
                        var req = _requestTree.Find(btnRelated.Tag.ToString());
                        if (req != null)
                        {
                            ShowRequestDetails(req);
                        }
                    };
                    pnlRequestDetails.Controls.Add(btnRelated);
                    y += 30;
                }
            }
        }

        private void AddDetailSection(string label, string value, ref int x, ref int y, int width)
        {
            if (string.IsNullOrEmpty(value)) return;

            var lblLabel = new Label
            {
                Text = label + ":",
                Location = new Point(x, y),
                Font = new Font(Font.FontFamily, 9, FontStyle.Bold),
                AutoSize = true
            };
            pnlRequestDetails.Controls.Add(lblLabel);
            y += 20;

            var lblValue = new Label
            {
                Text = value,
                Location = new Point(x + 10, y),
                AutoSize = true,
                MaximumSize = new Size(width - 20, 0)
            };
            pnlRequestDetails.Controls.Add(lblValue);
            y += lblValue.Height + 10;
        }

        private Color GetStatusColor(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.Submitted:
                    return Color.DodgerBlue;
                case RequestStatus.InProgress:
                    return Color.Orange;
                case RequestStatus.OnHold:
                    return Color.OrangeRed;
                case RequestStatus.Completed:
                    return Color.LimeGreen;
                case RequestStatus.Cancelled:
                    return Color.Gray;
                case RequestStatus.Reopened:
                    return Color.Purple;
                default:
                    return Color.Black;
            }
        }

        private Color GetPriorityColor(RequestPriority priority)
        {
            switch (priority)
            {
                case RequestPriority.Low:
                    return Color.Green;
                case RequestPriority.Medium:
                    return Color.Orange;
                case RequestPriority.High:
                    return Color.OrangeRed;
                case RequestPriority.Critical:
                    return Color.Red;
                default:
                    return Color.Black;
            }
        }
    }

    // Custom DataGridView column for progress bar
    public class DataGridViewProgressColumn : DataGridViewImageColumn
    {
        public DataGridViewProgressColumn()
        {
            CellTemplate = new DataGridViewProgressCell();
        }
    }

    public class DataGridViewProgressCell : DataGridViewImageCell
    {
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            return value ?? 0;
        }

        protected override void Paint(Graphics g, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            int progressVal = 0;
            if (value != null && int.TryParse(value.ToString(), out int val))
            {
                progressVal = Math.Max(0, Math.Min(100, val));
            }

            // Draw cell background
            base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, null, null, null,
                cellStyle, advancedBorderStyle, paintParts & ~DataGridViewPaintParts.ContentForeground);

            // Draw progress bar
            var progressBarRect = new Rectangle(cellBounds.X + 2, cellBounds.Y + 2, cellBounds.Width - 6, cellBounds.Height - 6);
            var progressFillRect = new Rectangle(progressBarRect.X, progressBarRect.Y, 
                (int)(progressBarRect.Width * (progressVal / 100.0)), progressBarRect.Height);

            // Background
            using (var backBrush = new SolidBrush(Color.FromArgb(224, 224, 224)))
            {
                g.FillRectangle(backBrush, progressBarRect);
            }

            // Progress fill
            using (var progressBrush = new SolidBrush(GetProgressColor(progressVal)))
            {
                g.FillRectangle(progressBrush, progressFillRect);
            }

            // Border
            g.DrawRectangle(Pens.Gray, progressBarRect);

            // Text
            string text = $"{progressVal}%";
            TextRenderer.DrawText(g, text, cellStyle.Font, cellBounds, cellStyle.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private Color GetProgressColor(int percentage)
        {
            if (percentage < 25) return Color.FromArgb(255, 128, 128); // Light red
            if (percentage < 50) return Color.FromArgb(255, 200, 128); // Orange
            if (percentage < 75) return Color.FromArgb(128, 200, 255); // Light blue
            return Color.FromArgb(128, 255, 128); // Light green
        }
    }
}
