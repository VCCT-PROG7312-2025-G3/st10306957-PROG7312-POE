using PROG7312_POE.Models;
using PROG7312_POE.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PROG7312_POE.Forms
{
    public partial class PriorityQueueForm : Form
    {
        private readonly IIssueService _issueService;
        private BindingSource _requestsBindingSource = new BindingSource();
        private List<ServiceRequest> _allRequests = new List<ServiceRequest>();

        public PriorityQueueForm(IIssueService issueService)
        {
            InitializeComponent();
            _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
            
            // Set up form properties
            this.Text = "Service Request Priority Queue";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);
            
            // Initialize UI
            SetupDataGridView();
            
            // Load initial data
            LoadPriorityQueue();
            
            // Set up auto-refresh timer
            var refreshTimer = new Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += (s, e) => LoadPriorityQueue();
            refreshTimer.Start();
        }

        private void SetupDataGridView()
        {
            // Clear existing columns and data
            dgvRequests.Columns.Clear();
            dgvRequests.DataSource = null;
            
            // Configure DataGridView properties
            dgvRequests.AutoGenerateColumns = false;
            dgvRequests.AllowUserToAddRows = false;
            dgvRequests.AllowUserToDeleteRows = false;
            dgvRequests.ReadOnly = true;
            dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRequests.MultiSelect = false;
            dgvRequests.RowHeadersVisible = false;
            dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRequests.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvRequests.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            
            // Set up columns
            var columns = new[]
            {
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "RequestId",
                    HeaderText = "Request ID",
                    Name = "colRequestId",
                    Width = 120,
                    FillWeight = 15f
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Title",
                    HeaderText = "Title",
                    Name = "colTitle",
                    Width = 200,
                    FillWeight = 30f
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Priority",
                    HeaderText = "Priority",
                    Name = "colPriority",
                    Width = 100,
                    FillWeight = 15f
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Status",
                    HeaderText = "Status",
                    Name = "colStatus",
                    Width = 100,
                    FillWeight = 15f
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "DateSubmitted",
                    HeaderText = "Submitted",
                    Name = "colDateSubmitted",
                    Width = 150,
                    FillWeight = 25f,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "g",
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    },
                    HeaderCell = new DataGridViewColumnHeaderCell
                    {
                        Style = new DataGridViewCellStyle 
                        { 
                            Alignment = DataGridViewContentAlignment.MiddleLeft 
                        }
                    }
                }
            };
            
            dgvRequests.Columns.AddRange(columns);
            
            // Set up row style
            dgvRequests.RowTemplate.Height = 28;
            
            // Set up selection changed event
            dgvRequests.SelectionChanged += (s, e) =>
            {
                btnProcessNext.Enabled = dgvRequests.SelectedRows.Count > 0;
                btnUpdatePriority.Enabled = dgvRequests.SelectedRows.Count > 0;
            };
            
            // Set initial button states
            btnProcessNext.Enabled = false;
            btnUpdatePriority.Enabled = false;
        }

        private async void LoadPriorityQueue()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnRefresh.Enabled = false;
                
                // Get all requests from the priority queue
                var requests = await _issueService.GetAllPriorityRequestsAsync();
                _allRequests = requests?.ToList() ?? new List<ServiceRequest>();
                
                // Update the data source
                _requestsBindingSource.DataSource = null;
                _requestsBindingSource.DataSource = _allRequests;
                dgvRequests.DataSource = _requestsBindingSource;
                
                // Update status in the form title
                this.Text = $"Service Request Priority Queue - {_allRequests.Count} requests";
                
                // Auto-size columns to fit content
                dgvRequests.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading priority queue: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n{ex.InnerException.Message}";
                }
                
                MessageBox.Show(errorMessage, "Error Loading Data", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private async void BtnProcessNext_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) return;
            
            try
            {
                var selectedRequest = dgvRequests.SelectedRows[0].DataBoundItem as ServiceRequest;
                if (selectedRequest == null) return;
                
                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Mark request '{selectedRequest.Title}' as processed?\n\n" +
                    $"ID: {selectedRequest.RequestId}\n" +
                    $"Priority: {selectedRequest.Priority}",
                    "Confirm Processing",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (confirmResult == DialogResult.Yes)
                {
                    // Remove from priority queue
                    bool success = await _issueService.RemoveRequestFromQueueAsync(selectedRequest.RequestId);
                    
                    if (success)
                    {
                        // Refresh the queue
                        LoadPriorityQueue();
                        
                        MessageBox.Show("Request processed successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to process the request. Please try again.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                var nextRequest = await _issueService.GetNextPriorityRequestAsync();
                if (nextRequest != null)
                {
                    var processResult = MessageBox.Show(
                    $"Process request {nextRequest.RequestId} ({nextRequest.Title})?\n" +
                    $"Priority: {nextRequest.Priority}\n" +
                    $"Submitted: {nextRequest.DateSubmitted:g}",
                    "Process Next Request",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                    if (processResult == DialogResult.Yes)
                    {
                        await _issueService.RemoveRequestFromQueueAsync(nextRequest.RequestId);
                        LoadPriorityQueue();
                        MessageBox.Show($"Request {nextRequest.RequestId} has been processed and removed from the queue.", 
                            "Request Processed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No requests in the queue.", "Queue Empty", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing next request: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUpdatePriority_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request to update.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRequest = dgvRequests.SelectedRows[0].DataBoundItem as ServiceRequest;
            if (selectedRequest == null) return;

            using (var dialog = new PriorityUpdateDialog(selectedRequest.Priority))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        bool success = await _issueService.UpdateRequestPriorityAsync(
                            selectedRequest.RequestId, 
                            dialog.SelectedPriority);
                        
                        if (success)
                        {
                            LoadPriorityQueue();
                            MessageBox.Show("Priority updated successfully.", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating priority: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadPriorityQueue();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    // Simple dialog for updating priority
    public class PriorityUpdateDialog : Form
    {
        private ComboBox cmbPriority;
        private Button btnOK;
        private Button btnCancel;

        public RequestPriority SelectedPriority { get; private set; }

        public PriorityUpdateDialog(RequestPriority currentPriority)
        {
            InitializeComponent();
            SelectedPriority = currentPriority;
            cmbPriority.SelectedItem = currentPriority;
        }

        private void InitializeComponent()
        {
            this.cmbPriority = new ComboBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            
            // cmbPriority
            this.cmbPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPriority.FormattingEnabled = true;
            this.cmbPriority.Items.AddRange(Enum.GetValues(typeof(RequestPriority)).Cast<object>().ToArray());
            this.cmbPriority.Location = new System.Drawing.Point(12, 25);
            this.cmbPriority.Name = "cmbPriority";
            this.cmbPriority.Size = new System.Drawing.Size(200, 21);
            this.cmbPriority.TabIndex = 0;
            
            // btnOK
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(56, 60);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += BtnOK_Click;
            
            // btnCancel
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(137, 60);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            
            // PriorityUpdateDialog
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(224, 95);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cmbPriority);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PriorityUpdateDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Update Priority";
            this.ResumeLayout(false);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (cmbPriority.SelectedItem != null)
            {
                SelectedPriority = (RequestPriority)cmbPriority.SelectedItem;
            }
        }
    }
}
