using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IFileService _fileService;

        public IssueService(IIssueRepository issueRepository, IFileService fileService)
        {
            _issueRepository = issueRepository;
            _fileService = fileService;
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
    }
}