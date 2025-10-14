using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PROG7312_POE.Models;

namespace PROG7312_POE.Services
{
    public class EventService : IEventService, IDisposable
    {
        private readonly object _lock = new object();
        private int _nextId = 1;
        
        // Primary data store
        private readonly Dictionary<int, Event> _events = new Dictionary<int, Event>();
        
        // Indexes for fast lookups
        private readonly SortedDictionary<DateTime, List<Event>> _eventsByDate = new SortedDictionary<DateTime, List<Event>>();
        private readonly Dictionary<string, HashSet<Event>> _eventsByCategory = new Dictionary<string, HashSet<Event>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<Event>> _eventsByOrganizer = new Dictionary<string, HashSet<Event>>(StringComparer.OrdinalIgnoreCase);
        
        // For thread-safe operations
        private readonly ConcurrentDictionary<int, object> _eventLocks = new ConcurrentDictionary<int, object>();

        public EventService()
        {
            // Initialize with some sample data for testing
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            Console.WriteLine("Initializing sample event data...");
            var now = DateTime.Now;
            var sampleEvents = new List<Event>
            {
                new Event
                {
                    Id = _nextId++,
                    Title = "Community Cleanup",
                    Description = "Help clean up the local park and surrounding areas.",
                    StartDate = now.AddDays(7),
                    EndDate = now.AddDays(7).AddHours(4),
                    Location = "Central Park",
                    Category = "Community",
                    Organizer = "City Council",
                    MaxAttendees = 50,
                    CurrentAttendees = 12,
                    ContactEmail = "events@citycouncil.gov",
                    ContactPhone = "(123) 456-7890"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Weekly Farmers Market",
                    Description = "Fresh local produce and handmade goods.",
                    StartDate = now.AddDays(2),
                    EndDate = now.AddDays(2).AddHours(6),
                    Location = "Downtown Square",
                    Category = "Market",
                    Organizer = "Local Farmers Association",
                    MaxAttendees = 200,
                    CurrentAttendees = 0,
                    ContactEmail = "farmers@local.org",
                    ContactPhone = "(123) 555-1234"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Tech Conference 2023",
                    Description = "Annual technology conference with workshops and keynotes.",
                    StartDate = now.AddDays(14),
                    EndDate = now.AddDays(16),
                    Location = "Convention Center",
                    Category = "Technology",
                    Organizer = "Tech Events Inc.",
                    MaxAttendees = 500,
                    CurrentAttendees = 243,
                    ContactEmail = "info@techevents.com",
                    ContactPhone = "(555) 123-4567"
                }
            };

            Console.WriteLine($"Adding {sampleEvents.Count} sample events...");
            foreach (var evt in sampleEvents)
            {
                AddEvent(evt);
                Console.WriteLine($"Added event: {evt.Title} (ID: {evt.Id}) on {evt.StartDate}");
            }
            
            Console.WriteLine($"Total events in service: {_events.Count}");
        }

        private void AddEvent(Event evt)
        {
            // Add to main dictionary
            _events[evt.Id] = evt;

            // Update date index
            var dateKey = evt.StartDate.Date;
            if (!_eventsByDate.TryGetValue(dateKey, out var dateEvents))
            {
                dateEvents = new List<Event>();
                _eventsByDate[dateKey] = dateEvents;
            }
            dateEvents.Add(evt);

            // Update category index
            if (!_eventsByCategory.TryGetValue(evt.Category, out var categoryEvents))
            {
                categoryEvents = new HashSet<Event>();
                _eventsByCategory[evt.Category] = categoryEvents;
            }
            categoryEvents.Add(evt);

            // Update organizer index
            if (!_eventsByOrganizer.TryGetValue(evt.Organizer, out var organizerEvents))
            {
                organizerEvents = new HashSet<Event>();
                _eventsByOrganizer[evt.Organizer] = organizerEvents;
            }
            organizerEvents.Add(evt);
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return _events.TryGetValue(id, out var evt) ? evt : null;
                }
            });
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await Task.Run(() => _events.Values.OrderBy(e => e.StartDate).ToList());
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await Task.Run(() =>
            {
                var now = DateTime.Now;
                return _events.Values
                    .Where(e => e.StartDate >= now)
                    .OrderBy(e => e.StartDate)
                    .ToList();
            });
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime? endDate = null)
        {
            return await Task.Run(() =>
            {
                var end = endDate ?? DateTime.MaxValue;
                return _eventsByDate
                    .Where(kvp => kvp.Key >= startDate.Date && kvp.Key <= end.Date)
                    .SelectMany(kvp => kvp.Value)
                    .Where(e => e.StartDate >= startDate && e.StartDate <= end)
                    .OrderBy(e => e.StartDate)
                    .ToList();
            });
        }

        public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(string category)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(category) || !_eventsByCategory.TryGetValue(category, out var events))
                {
                    return Enumerable.Empty<Event>();
                }
                return events.OrderBy(e => e.StartDate).ToList();
            });
        }

        public async Task<IEnumerable<string>> GetEventCategoriesAsync()
        {
            return await Task.Run(() => _eventsByCategory.Keys.OrderBy(k => k).ToList());
        }

        public async Task<bool> AddEventAsync(Event eventItem)
        {
            if (eventItem == null)
                return false;

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    eventItem.Id = _nextId++;
                    eventItem.CreatedAt = DateTime.Now;
                    AddEvent(eventItem);
                    return true;
                }
            });
        }

        public async Task<bool> UpdateEventAsync(Event eventItem)
        {
            if (eventItem == null || eventItem.Id <= 0)
                return false;

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_events.ContainsKey(eventItem.Id))
                        return false;

                    var existingEvent = _events[eventItem.Id];
                    
                    // Remove from indexes
                    RemoveFromIndexes(existingEvent);
                    
                    // Update properties
                    existingEvent.Title = eventItem.Title;
                    existingEvent.Description = eventItem.Description;
                    existingEvent.StartDate = eventItem.StartDate;
                    existingEvent.EndDate = eventItem.EndDate;
                    existingEvent.Location = eventItem.Location;
                    existingEvent.Category = eventItem.Category;
                    existingEvent.Organizer = eventItem.Organizer;
                    existingEvent.MaxAttendees = eventItem.MaxAttendees;
                    existingEvent.ContactEmail = eventItem.ContactEmail;
                    existingEvent.ContactPhone = eventItem.ContactPhone;
                    existingEvent.ImagePath = eventItem.ImagePath;
                    existingEvent.Price = eventItem.Price;
                    
                    // Add back to indexes
                    AddEvent(existingEvent);
                    
                    return true;
                }
            });
        }

        private void RemoveFromIndexes(Event evt)
        {
            // Remove from date index
            var dateKey = evt.StartDate.Date;
            if (_eventsByDate.TryGetValue(dateKey, out var dateEvents))
            {
                dateEvents.Remove(evt);
                if (dateEvents.Count == 0)
                {
                    _eventsByDate.Remove(dateKey);
                }
            }

            // Remove from category index
            if (_eventsByCategory.TryGetValue(evt.Category, out var categoryEvents))
            {
                categoryEvents.Remove(evt);
                if (categoryEvents.Count == 0)
                {
                    _eventsByCategory.Remove(evt.Category);
                }
            }

            // Remove from organizer index
            if (_eventsByOrganizer.TryGetValue(evt.Organizer, out var organizerEvents))
            {
                organizerEvents.Remove(evt);
                if (organizerEvents.Count == 0)
                {
                    _eventsByOrganizer.Remove(evt.Organizer);
                }
            }
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_events.TryGetValue(eventId, out var evt))
                        return false;

                    RemoveFromIndexes(evt);
                    return _events.Remove(eventId);
                }
            });
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEventsAsync();

            var term = searchTerm.Trim().ToLower();
            return await Task.Run(() =>
            {
                return _events.Values
                    .Where(e => e.Title.ToLower().Contains(term) || 
                               e.Description.ToLower().Contains(term) ||
                               e.Location.ToLower().Contains(term) ||
                               e.Organizer.ToLower().Contains(term) ||
                               e.Category.ToLower().Contains(term))
                    .OrderBy(e => e.StartDate)
                    .ToList();
            });
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(string organizer)
        {
            if (string.IsNullOrWhiteSpace(organizer))
                return Enumerable.Empty<Event>();

            return await Task.Run(() =>
            {
                return _eventsByOrganizer.TryGetValue(organizer, out var events)
                    ? events.OrderBy(e => e.StartDate).ToList()
                    : Enumerable.Empty<Event>();
            });
        }

        public async Task<bool> RegisterForEventAsync(int eventId, string attendeeName, string contactInfo)
        {
            if (string.IsNullOrWhiteSpace(attendeeName) || eventId <= 0)
                return false;

            return await Task.Run(() =>
            {
                var lockObj = _eventLocks.GetOrAdd(eventId, _ => new object());
                lock (lockObj)
                {
                    if (!_events.TryGetValue(eventId, out var evt) || 
                        evt.CurrentAttendees >= evt.MaxAttendees)
                        return false;

                    evt.CurrentAttendees++;
                    return true;
                }
            });
        }

        public async Task<bool> CancelRegistrationAsync(int eventId, string attendeeName)
        {
            if (string.IsNullOrWhiteSpace(attendeeName) || eventId <= 0)
                return false;

            return await Task.Run(() =>
            {
                var lockObj = _eventLocks.GetOrAdd(eventId, _ => new object());
                lock (lockObj)
                {
                    if (!_events.TryGetValue(eventId, out var evt) || evt.CurrentAttendees <= 0)
                        return false;

                    evt.CurrentAttendees--;
                    return true;
                }
            });
        }

        public async Task<int> GetEventCountAsync()
        {
            return await Task.Run(() => _events.Count);
        }

        public async Task<Dictionary<string, int>> GetEventsByCategoryCountAsync()
        {
            return await Task.Run(() =>
            {
                return _eventsByCategory.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Count);
            });
        }

        public async Task<bool> SaveEventsToFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(_events.Values, options);
                    File.WriteAllText(filePath, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> LoadEventsFromFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(filePath))
                    return false;

                try
                {
                    var json = File.ReadAllText(filePath);
                    var events = JsonSerializer.Deserialize<List<Event>>(json);

                    if (events == null)
                        return false;

                    lock (_lock)
                    {
                        // Clear existing data
                        _events.Clear();
                        _eventsByDate.Clear();
                        _eventsByCategory.Clear();
                        _eventsByOrganizer.Clear();

                        // Reset next ID
                        _nextId = 1;

                        // Add all events from file
                        foreach (var evt in events)
                        {
                            evt.Id = _nextId++;
                            AddEvent(evt);
                        }
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public void Dispose()
        {
            // Cleanup resources if needed
        }
    }
}
