namespace EarlyWarning.Models
{
    public class CommonAttribute
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LasModifiedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
