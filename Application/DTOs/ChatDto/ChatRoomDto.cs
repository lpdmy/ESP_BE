using EduSphere.Domain.Models;

namespace EduShpere.Application.DTOs.ChatDto
{
    public class ChatRoomDto
    {
        public string Id { get; set; }
        public List<int> ParticipantIds { get; set; }
        public List<string> ParticipantNames { get; set; }
        public List<string> ParticipantAvatars { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UnreadCount { get; set; } = 0;
    }

    public class ChatRoomDetailDto : ChatRoomDto
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
