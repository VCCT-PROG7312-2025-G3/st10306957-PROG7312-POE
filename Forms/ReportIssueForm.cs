using PROG7312_POE.Models;
using PROG7312_POE.Services;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PROG7312_POE.Forms
{
    public partial class ReportIssueForm : Form
    {
        private readonly IIssueService _issueService;
        private readonly IFileService _fileService;
        private string _attachmentPath;
        private ProgressBar progressBar;
        private Label progressLabel;
        private Button btnAttachFile;
        private Button btnSubmit;
        private Button btnCancel;
        private Panel attachmentPanel;
        private Panel buttonPanel;
        private TextBox txtLocation;
        private ComboBox cmbCategory;
        private TextBox txtDescription;
        private Label lblAttachment;

        public ReportIssueForm(IIssueService issueService, IFileService fileService)
        {
            _issueService = issueService;
            _fileService = fileService;
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Report an Issue";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new Padding(20);

            // Main container
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 40),  // Title
                    new RowStyle(SizeType.Absolute, 10),  // Spacer
                    new RowStyle(SizeType.Absolute, 60),  // Location
                    new RowStyle(SizeType.Absolute, 10),  // Spacer
                    new RowStyle(SizeType.Absolute, 60),  // Category
                    new RowStyle(SizeType.Absolute, 10),  // Spacer
                    new RowStyle(SizeType.Percent, 100),  // Description
                    new RowStyle(SizeType.Absolute, 60),  // Attachment
                    new RowStyle(SizeType.Absolute, 40),  // Progress
                    new RowStyle(SizeType.Absolute, 60)   // Buttons
                },
                Padding = new Padding(10)
            };

            // Add title
            var titleLabel = new Label
            {
                Text = "Report an Issue",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.SetColumnSpan(titleLabel, 2);

            // Add location controls
            mainPanel.Controls.Add(new Label { Text = "Location:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            txtLocation = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 5) };
            mainPanel.Controls.Add(txtLocation, 1, 2);

            // Add category controls
            mainPanel.Controls.Add(new Label { Text = "Category:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 4);
            cmbCategory = new ComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 5) };
            cmbCategory.Items.AddRange(new string[] { "Select a category", "Roads", "Electricity", "Water", "Sewage", "Other" });
            cmbCategory.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbCategory, 1, 4);

            // Add description controls
            mainPanel.Controls.Add(new Label { Text = "Description:", TextAlign = ContentAlignment.TopRight, Dock = DockStyle.Fill }, 0, 6);
            txtDescription = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainPanel.Controls.Add(txtDescription, 1, 6);

            // Add attachment controls
            attachmentPanel = new Panel { Dock = DockStyle.Fill };
            btnAttachFile = new Button
            {
                Text = "Attach File",
                Dock = DockStyle.Left,
                Size = new Size(100, 30),
                Margin = new Padding(0, 0, 10, 0)
            };
            btnAttachFile.Click += BtnAttachFile_Click;

            lblAttachment = new Label
            {
                Text = "No file selected",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };

            attachmentPanel.Controls.Add(lblAttachment);
            attachmentPanel.Controls.Add(btnAttachFile);
            mainPanel.Controls.Add(new Label { Text = "Attachment:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 7);
            mainPanel.Controls.Add(attachmentPanel, 1, 7);

            // Add progress bar
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 20,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            progressLabel = new Label
            {
                Text = "0% Complete",
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Top,
                Height = 20,
                Margin = new Padding(0, 10, 0, 5)
            };

            var progressContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 40
            };
            progressContainer.Controls.Add(progressLabel);
            progressContainer.Controls.Add(progressBar);
            mainPanel.Controls.Add(new Label { Text = "Progress:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 8);
            mainPanel.Controls.Add(progressContainer, 1, 8);

            // Add buttons
            buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                Height = 40,
                Width = 220
            };

            btnSubmit = new Button
            {
                Text = "Submit",
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 64, 122),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSubmit.Click += BtnSubmit_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(btnSubmit);
            buttonPanel.Controls.Add(btnCancel);
            btnCancel.Left = buttonPanel.Width - btnCancel.Width;
            btnSubmit.Left = btnCancel.Left - btnSubmit.Width - 10;

            var buttonContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 60
            };
            buttonContainer.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(buttonContainer, 0, 9);
            mainPanel.SetColumnSpan(buttonContainer, 2);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Set accept and cancel buttons
            this.AcceptButton = btnSubmit;
            this.CancelButton = btnCancel;

            // Wire up events for progress tracking
            txtLocation.TextChanged += (s, e) => UpdateProgress();
            cmbCategory.SelectedIndexChanged += (s, e) => UpdateProgress();
            txtDescription.TextChanged += (s, e) => UpdateProgress();

            // Initial progress update
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            int progress = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) progress += 25;
            if (cmbCategory.SelectedIndex > 0) progress += 25;
            if (!string.IsNullOrWhiteSpace(txtDescription.Text)) progress += 25;
            if (!string.IsNullOrEmpty(_attachmentPath)) progress += 25;

            progressBar.Value = progress;
            progressLabel.Text = $"{progress}% Complete";
        }

        private void BtnAttachFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _attachmentPath = openFileDialog.FileName;
                    lblAttachment.Text = Path.GetFileName(_attachmentPath);
                    UpdateProgress();
                }
            }
        }

        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLocation.Text) ||
                cmbCategory.SelectedIndex <= 0 ||
                string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSubmit.Enabled = false;

                var issue = new Issue
                {
                    Location = txtLocation.Text.Trim(),
                    Category = cmbCategory.Text,
                    Description = txtDescription.Text.Trim(),
                    Status = "New",
                    ReportedDate = DateTime.Now
                };

                if (!string.IsNullOrEmpty(_attachmentPath))
                {
                    issue.AttachmentFileName = Path.GetFileName(_attachmentPath);
                    issue.AttachmentData = File.ReadAllBytes(_attachmentPath);
                }

                await _issueService.SubmitIssueAsync(issue);

                MessageBox.Show("Your issue has been submitted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while submitting your issue: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }
    }
}