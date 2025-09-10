using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PROG7312_POE
{
    public class ReportIssueForm : Form
    {
        private TextBox txtLocation;
        private ComboBox cmbCategory;
        private RichTextBox txtDescription;
        private Button btnAttachFile;
        private Button btnSubmit;
        private Button btnBack;
        private Label lblAttachment;
        private ProgressBar progressBar;
        private Label lblProgress;
        private string attachedFilePath;
        private static IssueCollection reportedIssues = new IssueCollection();

        public ReportIssueForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Report an Issue";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Padding = new Padding(20);

            // Main table layout
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 200),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Percent, 100)
                },
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Location
            mainLayout.Controls.Add(CreateLabel("Location:"), 0, 0);
            txtLocation = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            mainLayout.Controls.Add(txtLocation, 1, 0);

            // Category
            mainLayout.Controls.Add(CreateLabel("Category:"), 0, 1);
            cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            
            cmbCategory.Items.Add("Sanitation");
            cmbCategory.Items.Add("Roads");
            cmbCategory.Items.Add("Utilities");
            cmbCategory.Items.Add("Parks");
            cmbCategory.Items.Add("Other");
            
            cmbCategory.SelectedIndex = 0;
            mainLayout.Controls.Add(cmbCategory, 1, 1);

            // Description
            mainLayout.Controls.Add(CreateLabel("Description:"), 0, 2);
            txtDescription = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            mainLayout.Controls.Add(txtDescription, 1, 2);

            // Attachment
            mainLayout.Controls.Add(CreateLabel("Attachment:"), 0, 3);
            Panel attachmentPanel = new Panel { Dock = DockStyle.Fill };
            btnAttachFile = new Button
            {
                Text = "Attach File",
                Dock = DockStyle.Left,
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };
            btnAttachFile.Click += BtnAttachFile_Click;
            lblAttachment = new Label
            {
                Text = "No file attached",
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 0, 0, 0)
            };
            attachmentPanel.Controls.Add(lblAttachment);
            attachmentPanel.Controls.Add(btnAttachFile);
            mainLayout.Controls.Add(attachmentPanel, 1, 3);

            // Progress Bar
            progressBar = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Continuous };
            mainLayout.Controls.Add(CreateLabel("Progress:"), 0, 4);
            mainLayout.Controls.Add(progressBar, 1, 4);

            // Progress Label
            lblProgress = new Label { Text = "Complete the form to submit", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            mainLayout.SetColumnSpan(lblProgress, 2);
            mainLayout.Controls.Add(lblProgress, 0, 5);

            // Buttons
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 50F),
                    new ColumnStyle(SizeType.Percent, 50F)
                },
                RowStyles = { new RowStyle(SizeType.Percent, 100F) }
            };

            btnBack = new Button
            {
                Text = "Back to Main Menu",
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 0, 5, 0),
                Font = new Font("Segoe UI", 10)
            };
            btnBack.Click += BtnBack_Click;

            btnSubmit = new Button
            {
                Text = "Submit Report",
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 0, 10, 0),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSubmit.Click += BtnSubmit_Click;

            buttonPanel.Controls.Add(btnBack, 0, 0);
            buttonPanel.Controls.Add(btnSubmit, 1, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 6);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // Add validation
            txtLocation.TextChanged += (s, e) => UpdateFormState();
            txtDescription.TextChanged += (s, e) => UpdateFormState();

            this.Controls.Add(mainLayout);
            UpdateFormState();
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Margin = new Padding(0, 10, 10, 10)
            };
        }

        private void BtnAttachFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Documents|*.pdf;*.doc;*.docx|All files|*.*";
                openFileDialog.Title = "Select a file to attach";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    attachedFilePath = openFileDialog.FileName;
                    lblAttachment.Text = Path.GetFileName(attachedFilePath);
                    UpdateFormState();
                }
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Simulate processing
                progressBar.Style = ProgressBarStyle.Marquee;
                btnSubmit.Enabled = false;
                lblProgress.Text = "Submitting your report...";

                // Create report object
                var report = new IssueReport
                {
                    Location = txtLocation.Text,
                    Category = cmbCategory.SelectedItem.ToString(),
                    Description = txtDescription.Text,
                    AttachmentPath = attachedFilePath,
                    ReportDate = DateTime.Now,
                    Status = "Submitted"
                };

                // Add to our custom collection
                reportedIssues.Add(report);
                
                // Simulate processing time
                System.Threading.Thread.Sleep(1500);

                MessageBox.Show("Your report has been submitted successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Style = ProgressBarStyle.Continuous;
                btnSubmit.Enabled = true;
                lblProgress.Text = "Error submitting report. Please try again.";
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void UpdateFormState()
        {
            bool isValid = !string.IsNullOrWhiteSpace(txtLocation.Text) &&
                          !string.IsNullOrWhiteSpace(txtDescription.Text);

            btnSubmit.Enabled = isValid;
            progressBar.Value = CalculateProgress();
            
            if (isValid)
            {
                lblProgress.Text = "Ready to submit";
                progressBar.Style = ProgressBarStyle.Continuous;
            }
            else
            {
                lblProgress.Text = "Please complete all required fields";
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        private int CalculateProgress()
        {
            int progress = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) progress += 25;
            if (!string.IsNullOrWhiteSpace(txtDescription.Text)) progress += 50;
            if (!string.IsNullOrWhiteSpace(attachedFilePath)) progress += 25;
            return progress;
        }
    }

    public class IssueReport
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime ReportDate { get; set; }
        public string Status { get; set; }
    }

    public class IssueCollection
    {
        private IssueReport[] reports;
        private int count;

        public IssueCollection()
        {
            reports = new IssueReport[10];
            count = 0;
        }

        public void Add(IssueReport report)
        {
            if (count == reports.Length)
            {
                IssueReport[] newReports = new IssueReport[reports.Length * 2];
                reports.CopyTo(newReports, 0);
                reports = newReports;
            }
            reports[count] = report;
            count++;
        }
    }
}
