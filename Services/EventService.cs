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
                // ===== COMMUNITY EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Community Cleanup Day",
                    Description = "Join us for a day of cleaning up our local park. Gloves and bags provided. All ages welcome!",
                    StartDate = now.AddDays(7),
                    EndDate = now.AddDays(7).AddHours(4),
                    Location = "Central Park, Main Entrance",
                    Category = "Community",
                    Organizer = "City Council",
                    MaxAttendees = 100,
                    CurrentAttendees = 45,
                    ContactEmail = "events@citycouncil.gov",
                    ContactPhone = "(123) 456-7890",
                    Price = 0,
                    ImagePath = "/images/cleanup.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Neighborhood Watch Meeting",
                    Description = "Monthly meeting to discuss community safety and neighborhood watch program updates.",
                    StartDate = now.AddDays(14).AddHours(18),
                    EndDate = now.AddDays(14).AddHours(20),
                    Location = "Community Center, Room 3",
                    Category = "Community",
                    Organizer = "Neighborhood Watch",
                    MaxAttendees = 50,
                    CurrentAttendees = 12,
                    ContactEmail = "safety@neighborhoodwatch.org",
                    ContactPhone = "(123) 555-1111",
                    Price = 0
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Food Drive for Local Shelter",
                    Description = "Help us collect non-perishable food items for the local homeless shelter.",
                    StartDate = now.AddDays(5),
                    EndDate = now.AddDays(12),
                    Location = "Various Drop-off Locations",
                    Category = "Community",
                    Organizer = "Helping Hands",
                    MaxAttendees = 500,
                    CurrentAttendees = 127,
                    ContactEmail = "donate@helpinghands.org",
                    ContactPhone = "(123) 555-2222",
                    Price = 0
                },

                // ===== MARKET EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Farmers Market - Summer Season",
                    Description = "Fresh local produce, artisanal foods, and handmade crafts from local vendors.",
                    StartDate = now.AddDays(2),
                    EndDate = now.AddDays(2).AddHours(6),
                    Location = "Downtown Square, Main Street",
                    Category = "Market",
                    Organizer = "Local Farmers Association",
                    MaxAttendees = 200,
                    CurrentAttendees = 150,
                    ContactEmail = "market@localfarmers.org",
                    ContactPhone = "(123) 555-1234",
                    Price = 0
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Holiday Craft Fair",
                    Description = "Handmade gifts, decorations, and treats from local artisans. Perfect for holiday shopping!",
                    StartDate = now.AddDays(30),
                    EndDate = now.AddDays(30).AddHours(8),
                    Location = "Community Center, Main Hall",
                    Category = "Market",
                    Organizer = "Artisan Collective",
                    MaxAttendees = 300,
                    CurrentAttendees = 87,
                    ContactEmail = "craftfair@artisans.org",
                    ContactPhone = "(123) 555-3333",
                    Price = 5.00m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Antique & Collectibles Market",
                    Description = "Vintage treasures, antiques, and collectibles from local dealers and collectors.",
                    StartDate = now.AddDays(16),
                    EndDate = now.AddDays(16).AddHours(7),
                    Location = "Old Town Square",
                    Category = "Market",
                    Organizer = "Vintage Finds",
                    MaxAttendees = 150,
                    CurrentAttendees = 64,
                    ContactEmail = "info@vintagefinds.com",
                    ContactPhone = "(123) 555-4444",
                    Price = 2.00m
                },

                // ===== TECHNOLOGY EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Tech Conference 2023",
                    Description = "Annual technology conference featuring industry leaders, workshops, and networking opportunities.",
                    StartDate = now.AddDays(14),
                    EndDate = now.AddDays(16),
                    Location = "Metro Convention Center",
                    Category = "Technology",
                    Organizer = "Tech Events Inc.",
                    MaxAttendees = 1000,
                    CurrentAttendees = 756,
                    ContactEmail = "info@techevents.com",
                    ContactPhone = "(555) 123-4567",
                    Price = 299.99m,
                    ImagePath = "/images/tech-conf.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Introduction to AI & Machine Learning",
                    Description = "Learn the fundamentals of artificial intelligence and machine learning in this hands-on workshop.",
                    StartDate = now.AddDays(8).AddHours(10),
                    EndDate = now.AddDays(8).AddHours(16),
                    Location = "Tech Hub, Room 101",
                    Category = "Technology",
                    Organizer = "AI Institute",
                    MaxAttendees = 40,
                    CurrentAttendees = 32,
                    ContactEmail = "workshops@ai-institute.org",
                    ContactPhone = "(555) 123-7890",
                    Price = 99.99m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Cybersecurity Workshop",
                    Description = "Learn how to protect yourself and your business from online threats and cyber attacks.",
                    StartDate = now.AddDays(22).AddHours(13),
                    EndDate = now.AddDays(22).AddHours(17),
                    Location = "Business Center, Security Lab",
                    Category = "Technology",
                    Organizer = "SecureNet Solutions",
                    MaxAttendees = 30,
                    CurrentAttendees = 18,
                    ContactEmail = "security@securenetsolutions.com",
                    ContactPhone = "(555) 123-4568",
                    Price = 149.99m
                },

                // ===== EDUCATION EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Introduction to Web Development",
                    Description = "Learn the basics of HTML, CSS, and JavaScript in this beginner-friendly workshop.",
                    StartDate = now.AddDays(5).AddHours(18),
                    EndDate = now.AddDays(5).AddHours(21),
                    Location = "Community College, Room 205",
                    Category = "Education",
                    Organizer = "Code Academy",
                    MaxAttendees = 30,
                    CurrentAttendees = 22,
                    ContactEmail = "workshops@codeacademy.org",
                    ContactPhone = "(555) 987-6543",
                    Price = 49.99m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Financial Literacy Workshop",
                    Description = "Learn essential money management skills, budgeting, and investment basics.",
                    StartDate = now.AddDays(12).AddHours(17.5),
                    EndDate = now.AddDays(12).AddHours(20),
                    Location = "Public Library, Meeting Room B",
                    Category = "Education",
                    Organizer = "Financial Freedom Inc.",
                    MaxAttendees = 40,
                    CurrentAttendees = 15,
                    ContactEmail = "learn@financialfreedom.org",
                    ContactPhone = "(555) 987-1234",
                    Price = 25.00m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Spanish for Beginners",
                    Description = "Start your journey to learning Spanish with our experienced language instructors.",
                    StartDate = now.AddDays(9).AddHours(18),
                    EndDate = now.AddDays(9).AddHours(20),
                    Location = "Language Center, Room 12",
                    Category = "Education",
                    Organizer = "Global Languages Institute",
                    MaxAttendees = 20,
                    CurrentAttendees = 8,
                    ContactEmail = "spanish@globallanguages.edu",
                    ContactPhone = "(555) 987-5678",
                    Price = 35.00m
                },

                // ===== SPORTS EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Annual City Marathon",
                    Description = "5K, 10K, and full marathon races through the city streets. All levels welcome!",
                    StartDate = now.AddDays(21),
                    EndDate = now.AddDays(21).AddHours(8),
                    Location = "City Hall, Starting Line",
                    Category = "Sports",
                    Organizer = "City Runners Club",
                    MaxAttendees = 2000,
                    CurrentAttendees = 1245,
                    ContactEmail = "marathon@cityrunners.org",
                    ContactPhone = "(123) 456-7891",
                    Price = 75.00m,
                    ImagePath = "/images/marathon.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Community Soccer Tournament",
                    Description = "Join us for a friendly 5-a-side soccer tournament. Teams or individual signups welcome!",
                    StartDate = now.AddDays(15),
                    EndDate = now.AddDays(15).AddHours(6),
                    Location = "Riverside Sports Complex, Field 3",
                    Category = "Sports",
                    Organizer = "City Sports League",
                    MaxAttendees = 100,
                    CurrentAttendees = 42,
                    ContactEmail = "soccer@citysports.org",
                    ContactPhone = "(123) 456-7892",
                    Price = 20.00m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Yoga in the Park - Special Charity Event",
                    Description = "Outdoor yoga session with all proceeds going to local mental health charities.",
                    StartDate = now.AddDays(28).AddHours(9),
                    EndDate = now.AddDays(28).AddHours(11),
                    Location = "Sunset Park, Main Lawn",
                    Category = "Sports",
                    Organizer = "Yoga for All",
                    MaxAttendees = 200,
                    CurrentAttendees = 87,
                    ContactEmail = "events@yogaforall.org",
                    ContactPhone = "(123) 456-7893",
                    Price = 15.00m,
                    ImagePath = "/images/yoga-charity.jpg"
                },

                // ===== MUSIC EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Summer Jazz Festival",
                    Description = "Weekend of live jazz performances featuring local and international artists.",
                    StartDate = now.AddDays(10),
                    EndDate = now.AddDays(12),
                    Location = "Riverside Park Amphitheater",
                    Category = "Music",
                    Organizer = "City Arts Council",
                    MaxAttendees = 5000,
                    CurrentAttendees = 3421,
                    ContactEmail = "jazzfest@cityarts.org",
                    ContactPhone = "(123) 555-6789",
                    Price = 45.00m,
                    ImagePath = "/images/jazz-fest.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Symphony Under the Stars",
                    Description = "Outdoor classical music performance by the City Philharmonic Orchestra.",
                    StartDate = now.AddDays(18).AddHours(19.5),
                    EndDate = now.AddDays(18).AddHours(22),
                    Location = "Botanical Gardens, Great Lawn",
                    Category = "Music",
                    Organizer = "City Philharmonic",
                    MaxAttendees = 1500,
                    CurrentAttendees = 876,
                    ContactEmail = "tickets@cityphil.org",
                    ContactPhone = "(123) 555-6790",
                    Price = 35.00m,
                    ImagePath = "/images/symphony.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Local Bands Night",
                    Description = "An evening showcasing the best local talent across various genres.",
                    StartDate = now.AddDays(7).AddHours(20),
                    EndDate = now.AddDays(7).AddHours(23.5),
                    Location = "The Underground Club",
                    Category = "Music",
                    Organizer = "Local Music Collective",
                    MaxAttendees = 250,
                    CurrentAttendees = 198,
                    ContactEmail = "bookings@localmusic.org",
                    ContactPhone = "(123) 555-6791",
                    Price = 15.00m
                },

                // ===== FOOD & DRINK EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Wine Tasting Evening",
                    Description = "Sample a selection of fine wines paired with artisanal cheeses.",
                    StartDate = now.AddDays(3).AddHours(19),
                    EndDate = now.AddDays(3).AddHours(22),
                    Location = "Vineyard Estate, Tasting Room",
                    Category = "Food & Drink",
                    Organizer = "Sunset Vineyards",
                    MaxAttendees = 40,
                    CurrentAttendees = 38,
                    ContactEmail = "events@sunsetvineyards.com",
                    ContactPhone = "(555) 234-5678",
                    Price = 65.00m,
                    ImagePath = "/images/wine-tasting.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Chocolate Making Workshop",
                    Description = "Learn the art of chocolate making from bean to bar with master chocolatiers.",
                    StartDate = now.AddDays(11).AddHours(14),
                    EndDate = now.AddDays(11).AddHours(17),
                    Location = "Cocoa Dreams Factory",
                    Category = "Food & Drink",
                    Organizer = "Chocolate Lovers",
                    MaxAttendees = 15,
                    CurrentAttendees = 12,
                    ContactEmail = "workshops@chocolatelovers.com",
                    ContactPhone = "(555) 234-5679",
                    Price = 85.00m,
                    ImagePath = "/images/chocolate-workshop.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Beer & Burger Festival",
                    Description = "Sample craft beers and gourmet burgers from local breweries and restaurants.",
                    StartDate = now.AddDays(25),
                    EndDate = now.AddDays(25).AddHours(6),
                    Location = "Riverside Park, North End",
                    Category = "Food & Drink",
                    Organizer = "City Food Events",
                    MaxAttendees = 800,
                    CurrentAttendees = 543,
                    ContactEmail = "info@cityfoodevents.com",
                    ContactPhone = "(555) 234-5680",
                    Price = 45.00m,
                    ImagePath = "/images/beer-festival.jpg"
                },

                // ===== HEALTH & WELLNESS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Yoga in the Park",
                    Description = "Beginner-friendly yoga session in the beautiful surroundings of Greenfield Park.",
                    StartDate = now.AddDays(1).AddHours(9),
                    EndDate = now.AddDays(1).AddHours(10.5),
                    Location = "Greenfield Park, South Lawn",
                    Category = "Health & Wellness",
                    Organizer = "Mind & Body Studio",
                    MaxAttendees = 50,
                    CurrentAttendees = 27,
                    ContactEmail = "hello@mindbodystudio.com",
                    ContactPhone = "(555) 345-6789",
                    Price = 15.00m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Meditation & Mindfulness Retreat",
                    Description = "A day of guided meditation, breathing exercises, and mindfulness practices.",
                    StartDate = now.AddDays(14).AddHours(9.5),
                    EndDate = now.AddDays(14).AddHours(16),
                    Location = "Tranquility Gardens",
                    Category = "Health & Wellness",
                    Organizer = "Peaceful Mind Center",
                    MaxAttendees = 30,
                    CurrentAttendees = 18,
                    ContactEmail = "retreats@peacefulmind.com",
                    ContactPhone = "(555) 345-6790",
                    Price = 99.00m,
                    ImagePath = "/images/meditation-retreat.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Nutrition & Healthy Cooking Class",
                    Description = "Learn to prepare delicious, healthy meals with a certified nutritionist.",
                    StartDate = now.AddDays(20).AddHours(18),
                    EndDate = now.AddDays(20).AddHours(21),
                    Location = "Culinary Arts Center, Kitchen 3",
                    Category = "Health & Wellness",
                    Organizer = "Healthy Living Institute",
                    MaxAttendees = 12,
                    CurrentAttendees = 8,
                    ContactEmail = "classes@healthyliving.org",
                    ContactPhone = "(555) 345-6791",
                    Price = 65.00m
                },

                // ===== BUSINESS EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Startup Networking Mixer",
                    Description = "Connect with local entrepreneurs, investors, and business professionals.",
                    StartDate = now.AddDays(8).AddHours(17.5),
                    EndDate = now.AddDays(8).AddHours(20),
                    Location = "Innovation Hub, 3rd Floor",
                    Category = "Business",
                    Organizer = "Startup City",
                    MaxAttendees = 150,
                    CurrentAttendees = 89,
                    ContactEmail = "events@startupcity.org",
                    ContactPhone = "(555) 456-7890",
                    Price = 25.00m
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Digital Marketing Masterclass",
                    Description = "Learn advanced digital marketing strategies from industry experts.",
                    StartDate = now.AddDays(15).AddHours(13),
                    EndDate = now.AddDays(15).AddHours(17),
                    Location = "Business Center, Conference Room A",
                    Category = "Business",
                    Organizer = "Marketing Pros",
                    MaxAttendees = 50,
                    CurrentAttendees = 32,
                    ContactEmail = "masterclass@marketingpros.com",
                    ContactPhone = "(555) 456-7891",
                    Price = 149.99m,
                    ImagePath = "/images/marketing-masterclass.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Women in Leadership Conference",
                    Description = "Inspiring talks and workshops for women in business and leadership roles.",
                    StartDate = now.AddDays(29),
                    EndDate = now.AddDays(30),
                    Location = "Grand Hotel, Crystal Ballroom",
                    Category = "Business",
                    Organizer = "Women Lead Network",
                    MaxAttendees = 300,
                    CurrentAttendees = 187,
                    ContactEmail = "conference@womenlead.org",
                    ContactPhone = "(555) 456-7892",
                    Price = 199.00m
                },

                // ===== FAMILY EVENTS =====
                new Event
                {
                    Id = _nextId++,
                    Title = "Children's Storytime & Crafts",
                    Description = "Interactive story reading followed by themed craft activities for children ages 3-8.",
                    StartDate = now.AddDays(4).AddHours(10),
                    EndDate = now.AddDays(4).AddHours(11.5),
                    Location = "City Public Library, Children's Section",
                    Category = "Family",
                    Organizer = "City Public Library",
                    MaxAttendees = 25,
                    CurrentAttendees = 18,
                    ContactEmail = "childrens@citylibrary.org",
                    ContactPhone = "(123) 555-3456",
                    Price = 0,
                    ImagePath = "/images/storytime.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Family Science Day",
                    Description = "Interactive science experiments and demonstrations for the whole family.",
                    StartDate = now.AddDays(17).AddHours(11),
                    EndDate = now.AddDays(17).AddHours(16),
                    Location = "Science Museum, All Floors",
                    Category = "Family",
                    Organizer = "City Science Center",
                    MaxAttendees = 400,
                    CurrentAttendees = 213,
                    ContactEmail = "events@sciencespot.org",
                    ContactPhone = "(123) 555-3457",
                    Price = 5.00m,
                    ImagePath = "/images/science-day.jpg"
                },
                new Event
                {
                    Id = _nextId++,
                    Title = "Outdoor Movie Night - Finding Nemo",
                    Description = "Family-friendly outdoor movie screening. Bring your own blankets and chairs!",
                    StartDate = now.AddDays(23).AddHours(19.5),
                    EndDate = now.AddDays(23).AddHours(21.5),
                    Location = "Community Park, West Field",
                    Category = "Family",
                    Organizer = "Parks & Recreation Department",
                    MaxAttendees = 500,
                    CurrentAttendees = 187,
                    ContactEmail = "events@cityparks.gov",
                    ContactPhone = "(123) 555-3458",
                    Price = 0,
                    ImagePath = "/images/movie-night.jpg"
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
