using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSheets.Gig.Entities
{
    internal class ExpenseEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string TripId { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;
        // Additional properties can be added as needed
    }
}
