using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PROG7312_POE.Models;
using PROG7312_POE.DataStructures;

namespace PROG7312_POE.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IFileService _fileService;
        private readonly RequestPriorityService _priorityService;

        public IssueService(IIssueRepository issueRepository, IFileService fileService, RequestPriorityService priorityService)
        {
            _issueRepository = issueRepository ?? throw new ArgumentNullException(nameof(issueRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _priorityService = priorityService ?? throw new ArgumentNullException(nameof(priorityService));
        }

        public async Task<bool> SubmitIssueAsync(Issue issue)
        {
            if (issue == null)
                throw new ArgumentNullException(nameof(issue));

            if (issue.Attachments != null && issue.Attachments.Count > 0)
            {
                var savedAttachments = new List<string>();
                foreach (var attachment in issue.Attachments)
                {
                    if (File.Exists(attachment))
                    {
                        var fileData = File.ReadAllBytes(attachment);
                        var savedPath = await _fileService.SaveFileAsync(attachment, fileData);
                        if (savedPath != null)
                        {
                            savedAttachments.Add(savedPath);
                        }
                    }
                }
                issue.Attachments = savedAttachments;
            }

            return await _issueRepository.AddIssueAsync(issue);
        }

        public Task<IEnumerable<Issue>> GetIssuesAsync()
        {
            return _issueRepository.GetAllIssuesAsync();
        }

        public Task<Issue> GetIssueByIdAsync(int id)
        {
            return _issueRepository.GetIssueByIdAsync(id);
        }

        public Task<bool> UpdateIssueStatusAsync(int id, string status)
        {
            return _issueRepository.UpdateIssueStatusAsync(id, status);
        }

        #region Priority Queue Implementation

        public async Task<ServiceRequest> GetNextPriorityRequestAsync()
        {
            try
            {
                return await Task.Run(() => _priorityService.GetNextRequest());
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error getting next priority request: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateRequestPriorityAsync(string requestId, RequestPriority newPriority)
        {
            try
            {
                return await Task.Run(() => _priorityService.UpdateRequestPriority(requestId, newPriority));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating request priority: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllPriorityRequestsAsync()
        {
            try
            {
                return await Task.Run(() => _priorityService.GetAllRequests());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all priority requests: {ex.Message}");
                return new List<ServiceRequest>();
            }
        }

        public async Task<bool> RemoveRequestFromQueueAsync(string requestId)
        {
            try
            {
                return await Task.Run(() => _priorityService.RemoveRequest(requestId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing request from queue: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}