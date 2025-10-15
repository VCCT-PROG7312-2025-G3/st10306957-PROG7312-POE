using System;
using System.Collections.Generic;

namespace PROG7312_POE.Models
{
    public class UserSearch
    {
        public int Id { get; set; }
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public DateTime SearchTime { get; set; } = DateTime.Now;
        public int? EventId { get; set; } // If the user viewed an event after searching
    }
}
