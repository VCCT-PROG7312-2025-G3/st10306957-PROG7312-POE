using System;

namespace PROG7312_POE.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Organizer { get; set; }
        public int MaxAttendees { get; set; }
        public int CurrentAttendees { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ImagePath { get; set; } 
        public decimal? Price { get; set; } 
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }
}
