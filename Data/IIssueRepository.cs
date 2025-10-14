using PROG7312_POE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PROG7312_POE
{
    public interface IIssueRepository
    {
        Task<bool> AddIssueAsync(Issue issue);
        Task<IEnumerable<Issue>> GetAllIssuesAsync();
        Task<Issue> GetIssueByIdAsync(int id);
        Task<bool> UpdateIssueStatusAsync(int id, string status);
    }
}
