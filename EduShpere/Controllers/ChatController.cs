using EduShpere.Application;
using EduShpere.Application.Services.ChatService;
using EduShpere.Shared.Constants;
using EduSphere.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHttpContextService _httpContextService;
        public ChatController(IChatService chatService, IHttpContextService httpContextService)
        {
            _chatService = chatService;
            _httpContextService = httpContextService;
        }

        [HttpGet(ApiEndpoints.Chat.Rooms)]
        public async Task<IActionResult> GetRooms()
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var rooms = await _chatService.GetUserRoomsWithNames((int) userId);
            return Ok(rooms);
        }

        [HttpGet(ApiEndpoints.Chat.GetMessages)]
        public async Task<IActionResult> GetMessages(string roomId, [FromQuery] int limit = 50)
        {
            var messages = await _chatService.GetRoomDetail(roomId, limit);
            return Ok(messages);
        }

        [HttpPost(ApiEndpoints.Chat.Rooms)]
        public async Task<IActionResult> CreateRoom([FromBody] ChatRoom room)
        {
            var created = await _chatService.CreateRoom(room);
            return Ok(created);
        }

        [HttpPost(ApiEndpoints.Chat.GetMessages)]
        public async Task<IActionResult> SendMessage(string roomId, [FromBody] ChatMessage message)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            message.SenderId = (int)userId;
            message.Timestamp = DateTime.UtcNow;
            message.RoomId = roomId;
            var saved = await _chatService.SendMessage(message);
            return Ok(saved);
        }

        [HttpPost(ApiEndpoints.Chat.MarkAsRead)]
        public async Task<IActionResult> MarkAsRead(string roomId)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _chatService.MarkMessagesAsRead(roomId, (int) userId);
            return Ok();
        }
    }
}
