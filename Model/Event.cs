namespace Namaya.Model
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string Location { get; set; }
        public string Status { get; set; } = "Upcoming";
        public string? ImagePath { get; set; }
    }
}