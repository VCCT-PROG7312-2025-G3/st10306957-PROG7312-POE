using System;
using System.Collections.Generic;

namespace PROG7312_POE.Models
{
    public class ServiceRequest
    {
        public string RequestId { get; set; }
        public string IssueId { get; set; }
        public RequestStatus Status { get; set; }
        public RequestPriority Priority { get; set; }
        public string AssignedDepartment { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public int ProgressPercentage { get; set; }
        public List<string> DependentRequestIds { get; set; } = new List<string>();
        public List<StatusUpdate> StatusHistory { get; set; } = new List<StatusUpdate>();
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string SubmittedBy { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Notes { get; set; }

        public ServiceRequest()
        {
            RequestId = GenerateRequestId();
            DateSubmitted = DateTime.Now;
            Status = RequestStatus.Submitted;
            Priority = RequestPriority.Medium;
            ProgressPercentage = 0;
            StatusHistory.Add(new StatusUpdate 
            { 
                Status = RequestStatus.Submitted, 
                Timestamp = DateTime.Now,
                Notes = "Request submitted" 
            });
        }

        private string GenerateRequestId()
        {
            return $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        public void UpdateStatus(RequestStatus newStatus, string notes = null)
        {
            var oldStatus = Status;
            Status = newStatus;
            
            StatusHistory.Add(new StatusUpdate
            {
                Status = newStatus,
                Timestamp = DateTime.Now,
                Notes = notes ?? $"Status changed from {oldStatus} to {newStatus}",
                ChangedBy = Environment.UserName
            });

            // Update progress based on status
            switch (newStatus)
            {
                case RequestStatus.Submitted:
                    ProgressPercentage = 0;
                    break;
                case RequestStatus.InProgress:
                    ProgressPercentage = 25;
                    break;
                case RequestStatus.OnHold:
                    ProgressPercentage = 50;
                    break;
                case RequestStatus.Completed:
                    ProgressPercentage = 100;
                    ActualCompletionDate = DateTime.Now;
                    break;
                case RequestStatus.Cancelled:
                    ProgressPercentage = 0;
                    break;
            }
        }
    }

    public enum RequestStatus
    {
        Submitted,
        InProgress,
        OnHold,
        Completed,
        Cancelled,
        Reopened
    }

    public enum RequestPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class StatusUpdate
    {
        public RequestStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string Notes { get; set; }
        public string ChangedBy { get; set; }
    }
}
