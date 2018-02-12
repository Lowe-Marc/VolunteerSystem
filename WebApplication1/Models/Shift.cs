using System;
namespace WebApplication1.Models
{
    public class Shift
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public Shift()
        {
        }
    }
}
