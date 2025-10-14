using System;
using System.Collections.Generic;

namespace PROG7312_POE.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Status { get; set; } = "Reported";
        public DateTime ReportedDate { get; set; } = DateTime.Now;
        public List<string> Attachments { get; set; } = new List<string>();
        public string ReporterName { get; set; }
        public string ReporterContact { get; set; }
        
        // Add attachment properties
        public string AttachmentFileName { get; set; }
        public byte[] AttachmentData { get; set; }
    }
}
