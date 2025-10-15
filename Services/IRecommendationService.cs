using System.Collections.Generic;
using System.Threading.Tasks;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    public interface IRecommendationService
    {
        Task RecordSearchAsync(string searchTerm, string category, string location, int? eventId = null);
        Task<List<Event>> GetRecommendedEventsAsync(string userId = null);
        Task<List<Event>> GetSimilarEventsAsync(Event targetEvent, int count = 5);
    }
}
