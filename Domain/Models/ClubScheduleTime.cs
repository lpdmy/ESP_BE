
namespace EduShpere.Domain.Models
{
    public class ClubScheduleItem
    {
        public List<int> DayOfWeek { get; set; } = new();
        public string Time { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
