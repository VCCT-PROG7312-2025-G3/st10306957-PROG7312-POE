using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    public class UserInteractionTracker
    {
        private const int MaxInteractionsPerUser = 1000;
        private readonly ConcurrentDictionary<string, List<UserInteraction>> _userInteractions = new();

        public void TrackInteraction(string userId, string interactionType, string eventId = null, string searchTerm = null, string category = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var interaction = new UserInteraction
            {
                UserId = userId,
                Type = Enum.Parse<InteractionType>(interactionType),
                EventId = eventId,
                SearchTerm = searchTerm,
                Category = category,
                Timestamp = DateTime.UtcNow
            };

            _userInteractions.AddOrUpdate(
                userId,
                new List<UserInteraction> { interaction },
                (_, interactions) =>
                {
                    interactions.Add(interaction);
                    if (interactions.Count > MaxInteractionsPerUser)
                    {
                        interactions.RemoveRange(0, interactions.Count - MaxInteractionsPerUser);
                    }
                    return interactions;
                });
        }

        public IEnumerable<UserInteraction> GetUserInteractions(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !_userInteractions.TryGetValue(userId, out var interactions))
            {
                return Enumerable.Empty<UserInteraction>();
            }
            return interactions.AsReadOnly();
        }

        public List<string> GetUserCategories(string userId)
        {
            if (!_userInteractions.TryGetValue(userId, out var interactions))
                return new List<string>();

            return interactions
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .GroupBy(i => i.Category)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToList();
        }

        public List<string> GetRecentSearchTerms(string userId, int count = 5)
        {
            if (!_userInteractions.TryGetValue(userId, out var interactions))
                return new List<string>();

            return interactions
                .Where(i => i.Type == InteractionType.Search && !string.IsNullOrEmpty(i.SearchTerm))
                .OrderByDescending(i => i.Timestamp)
                .Select(i => i.SearchTerm)
                .Distinct()
                .Take(count)
                .ToList();
        }
    }

    public class UserInteraction
    {
        public string UserId { get; set; }
        public InteractionType Type { get; set; }
        public string EventId { get; set; }
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
