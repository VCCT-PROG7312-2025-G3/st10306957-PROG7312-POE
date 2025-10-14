using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PROG7312_POE.Models;

namespace PROG7312_POE.Data
{
    public class IssueRepository : IIssueRepository
    {
        private readonly string _filePath;
        private List<Issue> _issues;

        public IssueRepository()
        {
            _filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MunicipalityIssueTracker",
                "issues.json");
            _issues = LoadIssues();
        }

        public Task<bool> AddIssueAsync(Issue issue)
        {
            _issues.Add(issue);
            SaveIssues();
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Issue>> GetAllIssuesAsync()
        {
            return Task.FromResult(_issues.AsEnumerable());
        }

        public Task<Issue> GetIssueByIdAsync(int id)
        {
            return Task.FromResult(_issues.FirstOrDefault(i => i.Id == id));
        }

        public Task<bool> UpdateIssueStatusAsync(int id, string status)
        {
            var issue = _issues.FirstOrDefault(i => i.Id == id);
            if (issue != null)
            {
                issue.Status = status;
                SaveIssues();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private List<Issue> LoadIssues()
        {
            if (!File.Exists(_filePath))
                return new List<Issue>();

            var json = File.ReadAllText(_filePath);
            return string.IsNullOrEmpty(json)
                ? new List<Issue>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Issue>>(json)
                    ?? new List<Issue>();
        }

        private void SaveIssues()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = System.Text.Json.JsonSerializer.Serialize(_issues, options);
            File.WriteAllText(_filePath, json);
        }
    }
}