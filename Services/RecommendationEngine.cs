using System;
using System.Collections.Generic;
using System.Linq;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    public class RecommendationEngine
    {
        private readonly UserInteractionTracker _interactionTracker;
        private readonly IEventService _eventService;
        private const int MaxRecommendations = 5;
        private const int RecentInteractionsToConsider = 20;
        private const double CategoryWeight = 0.5;
        private const double PopularityWeight = 0.3;
        private const double RecencyWeight = 0.2;

        public RecommendationEngine(UserInteractionTracker interactionTracker, IEventService eventService)
        {
            _interactionTracker = interactionTracker ?? throw new ArgumentNullException(nameof(interactionTracker));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task<IEnumerable<Event>> GetRecommendedEventsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return await GetPopularEventsAsync();

            var userInteractions = _interactionTracker.GetUserInteractions(userId)
                .OrderByDescending(i => i.Timestamp)
                .Take(RecentInteractionsToConsider)
                .ToList();

            if (!userInteractions.Any())
                return await GetPopularEventsAsync();

            var userPreferences = AnalyzeUserPreferences(userInteractions);
            var allEvents = (await _eventService.GetAllEventsAsync())
                .Where(e => e.StartDate >= DateTime.Now && e.IsActive)
                .ToList();

            if (!allEvents.Any())
                return Enumerable.Empty<Event>();

            var scoredEvents = allEvents.Select(e => new
            {
                Event = e,
                Score = CalculateEventScore(e, userPreferences, userInteractions)
            });

            return scoredEvents
                .OrderByDescending(x => x.Score)
                .Take(MaxRecommendations)
                .Select(x => x.Event);
        }

        private async Task<IEnumerable<Event>> GetPopularEventsAsync()
        {
            // Get upcoming events and order by popularity (using CurrentAttendees as a proxy)
            return (await _eventService.GetUpcomingEventsAsync())
                .OrderByDescending(e => e.CurrentAttendees)
                .Take(MaxRecommendations);
        }

        private Dictionary<string, int> AnalyzeUserPreferences(IEnumerable<UserInteraction> interactions)
        {
            var preferences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var interaction in interactions)
            {
                if (!string.IsNullOrEmpty(interaction.Category))
                {
                    if (preferences.ContainsKey(interaction.Category))
                        preferences[interaction.Category]++;
                    else
                        preferences[interaction.Category] = 1;
                }

                // Add weight to event views
                if (interaction.Type == InteractionType.EventView && !string.IsNullOrEmpty(interaction.Category))
                {
                    if (preferences.ContainsKey(interaction.Category))
                        preferences[interaction.Category] += 2; // Higher weight for viewed events
                    else
                        preferences[interaction.Category] = 2;
                }
            }

            return preferences;
        }

        private float CalculateEventScore(Event eventItem, Dictionary<string, int> preferences, List<UserInteraction> recentInteractions)
        {
            float score = 0f;
            
            // Category match score
            if (!string.IsNullOrEmpty(eventItem.Category) && 
                preferences.TryGetValue(eventItem.Category, out var categoryWeight))
            {
                score += categoryWeight * CategoryWeight;
            }

            // Popularity score (normalized)
            var maxAttendees = Math.Max(1, _eventService.GetAllEventsAsync().Result
                .Max(e => e.CurrentAttendees));

            score += ((float)eventItem.CurrentAttendees / maxAttendees) * PopularityWeight;

            // Recency score (prefer newer events)
            var daysUntilEvent = (eventItem.StartDate - DateTime.Now).TotalDays;
            var maxDays = 30; // Consider events within 30 days
            var recencyScore = 1 - Math.Min(1, daysUntilEvent / maxDays);
            score += (float)recencyScore * RecencyWeight;

            // Boost score if the user has viewed this event before
            if (recentInteractions.Any(i => 
                i.Type == InteractionType.EventView && 
                i.EventId == eventItem.Id.ToString()))
            {
                score *= 1.5f; // 50% boost for previously viewed events
            }

            return score;
        }
    }
}
