using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PROG7312_POE.Models;
using System.Text;

namespace PROG7312_POE.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly string _storagePath;
        private readonly IEventService _eventService;
        private List<UserSearch> _searchHistory;
        private const string SearchHistoryFile = "user_searches.json";
        private readonly object _fileLock = new object();

        public RecommendationService(IEventService eventService, string storagePath = null)
        {
            _eventService = eventService;
            _storagePath = storagePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(_storagePath);
            _searchHistory = LoadSearchHistory();
        }

        public async Task RecordSearchAsync(string searchTerm, string category, string location, int? eventId = null)
        {
            var search = new UserSearch
            {
                SearchTerm = searchTerm,
                Category = category,
                Location = location,
                EventId = eventId,
                SearchTime = DateTime.Now
            };

            _searchHistory.Add(search);
            SaveSearchHistory();
            await Task.CompletedTask;
        }

        public async Task<List<Event>> GetRecommendedEventsAsync(string userId = null)
        {
            // Get all active events
            var allEvents = await _eventService.GetActiveEventsAsync();
            if (!allEvents.Any()) return new List<Event>();

            // If no search history, return random events
            if (!_searchHistory.Any())
                return allEvents.OrderBy(e => Guid.NewGuid()).Take(5).ToList();

            // Get user's frequent categories and locations
            var userCategories = _searchHistory
                .Where(s => !string.IsNullOrEmpty(s.Category))
                .GroupBy(s => s.Category)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var userLocations = _searchHistory
                .Where(s => !string.IsNullOrEmpty(s.Location))
                .GroupBy(s => s.Location)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            // Score events based on user preferences
            var scoredEvents = allEvents.Select(e => new
            {
                Event = e,
                Score = CalculateEventScore(e, userCategories, userLocations)
            });

            // Return top 5 recommended events
            return scoredEvents
                .OrderByDescending(x => x.Score)
                .Take(5)
                .Select(x => x.Event)
                .ToList();
        }

        public async Task<List<Event>> GetSimilarEventsAsync(Event targetEvent, int count = 5)
        {
            var allEvents = await _eventService.GetActiveEventsAsync();
            
            return allEvents
                .Where(e => e.Id != targetEvent.Id) // Exclude the target event
                .Select(e => new
                {
                    Event = e,
                    SimilarityScore = CalculateSimilarityScore(targetEvent, e)
                })
                .OrderByDescending(x => x.SimilarityScore)
                .Take(count)
                .Select(x => x.Event)
                .ToList();
        }

        private int CalculateEventScore(Event e, List<string> preferredCategories, List<string> preferredLocations)
        {
            int score = 0;

            // Higher score for matching preferred categories
            if (preferredCategories.Contains(e.Category))
                score += 3;

            // Higher score for matching preferred locations
            if (preferredCategories.Any(c => string.Compare(c, e.Category, StringComparison.OrdinalIgnoreCase) == 0))
                score += 3;

            if (preferredLocations.Any(loc => e.Location.IndexOf(loc, StringComparison.OrdinalIgnoreCase) >= 0))
                score += 2;

            // Recent events get higher scores
            if ((DateTime.Now - e.StartDate).TotalDays <= 7) // Within a week
                score += 2;
            else if ((DateTime.Now - e.StartDate).TotalDays <= 30) // Within a month
                score += 1;

            return score;
        }

        private double CalculateSimilarityScore(Event event1, Event event2)
        {
            if (event1 == null || event2 == null) return 0;

            double score = 0;

            // Category match (case-insensitive)
            if (string.Equals(event1.Category, event2.Category, StringComparison.OrdinalIgnoreCase))
                score += 0.5;

            // Location similarity (case-insensitive partial match)
            if (!string.IsNullOrWhiteSpace(event1.Location) && !string.IsNullOrWhiteSpace(event2.Location))
            {
                if (event1.Location.IndexOf(event2.Location, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    event2.Location.IndexOf(event1.Location, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 0.3;
                }
            }

            // Date proximity (closer dates get higher scores)
            var daysDifference = Math.Abs((event1.StartDate - event2.StartDate).TotalDays);
            if (daysDifference <= 7) // Within a week
                score += 0.2;
            else if (daysDifference <= 30) // Within a month
                score += 0.1;

            // Ensure score is between 0 and 1.0
            return Math.Min(Math.Max(score, 0), 1.0);
        }

        private List<UserSearch> LoadSearchHistory()
        {
            var filePath = Path.Combine(_storagePath, SearchHistoryFile);
            if (!File.Exists(filePath))
                return new List<UserSearch>();

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<UserSearch>>(json) ?? new List<UserSearch>();
            }
            catch
            {
                return new List<UserSearch>();
            }
        }

        private void SaveSearchHistory()
        {
            var filePath = Path.Combine(_storagePath, SearchHistoryFile);
            var json = JsonSerializer.Serialize(_searchHistory, new JsonSerializerOptions { WriteIndented = true });
            
            lock (_fileLock)
            {
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
        }
    }
}
