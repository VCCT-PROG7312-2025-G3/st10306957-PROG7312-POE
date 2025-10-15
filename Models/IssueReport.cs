using System;
using System.Collections.Generic;

namespace PROG7312_POE.Models
{
    public class IssueReport
    {
        public int Id { get; set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Location { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public string Status { get; set; } = "New";
        public DateTime ReportedDate { get; set; } = DateTime.Now;
        public List<string> Attachments { get; } = new List<string>();
        public string ReporterName { get; private set; } = string.Empty;
        public string ReporterContact { get; private set; } = string.Empty;
        public int Priority { get; set; } = 3; // 1: High, 2: Medium, 3: Low

        // Private parameterless constructor to prevent direct instantiation without required parameters
        private IssueReport() { }

        public IssueReport(string title, string description, string location, string category,
                          string reporterName, string reporterContact)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            ReporterName = reporterName ?? throw new ArgumentNullException(nameof(reporterName));
            ReporterContact = reporterContact ?? throw new ArgumentNullException(nameof(reporterContact));
        }

        // Factory method for creating an issue report with minimal required fields
        public static IssueReport Create(string title, string description, string location, string category,
                                       string reporterName, string reporterContact)
        {
            return new IssueReport(title, description, location, category, reporterName, reporterContact);
        }

        // Method to add an attachment
        public void AddAttachment(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            Attachments.Add(filePath);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IssueReport report) return false;
            
            return Id == report.Id &&
                   Title == report.Title &&
                   Description == report.Description &&
                   Location == report.Location &&
                   Category == report.Category &&
                   Status == report.Status &&
                   ReportedDate == report.ReportedDate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (Title?.GetHashCode() ?? 0);
                hash = hash * 23 + (Description?.GetHashCode() ?? 0);
                hash = hash * 23 + (Location?.GetHashCode() ?? 0);
                hash = hash * 23 + (Category?.GetHashCode() ?? 0);
                hash = hash * 23 + (Status?.GetHashCode() ?? 0);
                hash = hash * 23 + ReportedDate.GetHashCode();
                return hash;
            }
        }
    }
}
