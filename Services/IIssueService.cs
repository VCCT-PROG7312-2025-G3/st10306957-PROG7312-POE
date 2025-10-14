using PROG7312_POE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROG7312_POE.Services
{
    public interface IIssueService
    {
        Task<bool> SubmitIssueAsync(Issue issue);
        Task<IEnumerable<Issue>> GetIssuesAsync();
        Task<Issue> GetIssueByIdAsync(int id);
        Task<bool> UpdateIssueStatusAsync(int id, string status);
    }
}
