namespace Namaya.Model
{
    public class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int StudentId { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}