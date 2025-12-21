using EduShpere.Application;
using EduShpere.Application.DTOs.WeeklyQuizDto;
using EduShpere.Application.Services.WeeklyQuizService;
using EduShpere.Application.Services;
using EduShpere.Infrastructure;
using EduShpere.Middlewares;
using EduShpere.Shared;
using EduShpere.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduShpere.Controllers
{
    [ApiController]
    [Route("api/weekly-quiz")]
    [CustomModelValidationFilter]
    public class WeeklyQuizController : BaseController
    {
        private readonly IWeeklyQuizService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;

        public WeeklyQuizController(
            IWeeklyQuizService service,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
        {
            _service = service;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Tạo quiz mới (Teacher/Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateWeeklyQuizDto dto)
        {
            var teacherId = _currentUserService.GetCurrentUserId() ?? 0;
            var teacher = await _userRepository.GetByIdAsync(teacherId);
            var teacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : null;

            var result = await _service.CreateAsync(dto, teacherId, teacherName);
            return Ok(new ResponseDto<WeeklyQuizResponseDto>(result, "Tạo quiz thành công"));
        }

        /// <summary>
        /// Lấy quiz theo ID (Student: không có đáp án, Teacher/Admin: có đáp án)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = _currentUserService.GetCurrentUserId() ?? 0;
            var user = await _userRepository.GetByIdAsync(userId);
            var isTeacherOrAdmin = user != null && (user.Role == UserRole.Admin || user.Role == UserRole.Teacher);

            var result = await _service.GetByIdAsync(id, includeCorrectAnswers: isTeacherOrAdmin);
            if (result == null)
            {
                return NotFound(new ResponseDto<string>(null, "Quiz không tồn tại", 404));
            }

            return Ok(new ResponseDto<WeeklyQuizResponseDto>(result, "Lấy quiz thành công"));
        }

        /// <summary>
        /// Lấy danh sách quiz theo tuần và năm
        /// </summary>
        [HttpGet("week/{weekNumber}/year/{year}")]
        [Authorize]
        public async Task<IActionResult> GetByWeekAndYear(int weekNumber, int year)
        {
            var result = await _service.GetByWeekAndYearAsync(weekNumber, year);
            return Ok(new ResponseDto<List<WeeklyQuizResponseDto>>(result, "Lấy danh sách quiz thành công"));
        }

        /// <summary>
        /// Lấy tất cả quiz active
        /// </summary>
        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllActiveAsync();
            return Ok(new ResponseDto<List<WeeklyQuizResponseDto>>(result, "Lấy danh sách quiz thành công"));
        }

        /// <summary>
        /// Lấy quiz của giáo viên (Teacher/Admin)
        /// </summary>
        [HttpGet("my-quizzes")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var teacherId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.GetByTeacherAsync(teacherId);
            return Ok(new ResponseDto<List<WeeklyQuizResponseDto>>(result, "Lấy danh sách quiz thành công"));
        }

        /// <summary>
        /// Cập nhật quiz (Teacher/Admin - chỉ người tạo)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateWeeklyQuizDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new ResponseDto<string>(null, "ID không khớp", 400));
            }

            var teacherId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.UpdateAsync(dto, teacherId);
            if (result == null)
            {
                return NotFound(new ResponseDto<string>(null, "Quiz không tồn tại hoặc bạn không có quyền sửa", 404));
            }

            return Ok(new ResponseDto<WeeklyQuizResponseDto>(result, "Cập nhật quiz thành công"));
        }

        /// <summary>
        /// Xóa quiz (Teacher/Admin - chỉ người tạo)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var teacherId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.DeleteAsync(id, teacherId);
            if (!result)
            {
                return NotFound(new ResponseDto<string>(null, "Quiz không tồn tại hoặc bạn không có quyền xóa", 404));
            }

            return Ok(new ResponseDto<bool>(true, "Xóa quiz thành công"));
        }

        /// <summary>
        /// Nộp bài quiz (Student)
        /// </summary>
        [HttpPost("submit")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto dto)
        {
            var studentId = _currentUserService.GetCurrentUserId() ?? 0;
            var student = await _userRepository.GetByIdAsync(studentId);
            var studentName = student != null ? $"{student.FirstName} {student.LastName}" : null;

            var result = await _service.SubmitQuizAsync(dto, studentId, studentName);
            return Ok(new ResponseDto<QuizSubmissionResponseDto>(result, "Nộp bài thành công"));
        }

        /// <summary>
        /// Lấy kết quả làm bài của học sinh
        /// </summary>
        [HttpGet("{quizId}/my-submission")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMySubmission(string quizId)
        {
            var studentId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.GetSubmissionAsync(quizId, studentId);
            if (result == null)
            {
                return NotFound(new ResponseDto<string>(null, "Bạn chưa làm quiz này", 404));
            }

            return Ok(new ResponseDto<QuizSubmissionResponseDto>(result, "Lấy kết quả thành công"));
        }

        /// <summary>
        /// Kiểm tra đã làm quiz chưa
        /// </summary>
        [HttpGet("{quizId}/check-submitted")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> CheckSubmitted(string quizId)
        {
            var studentId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.HasStudentSubmittedAsync(quizId, studentId);
            return Ok(new ResponseDto<bool>(result, result ? "Đã làm quiz" : "Chưa làm quiz"));
        }

        /// <summary>
        /// Lấy danh sách kết quả của quiz (Teacher/Admin)
        /// </summary>
        [HttpGet("{quizId}/submissions")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetSubmissions(string quizId)
        {
            var result = await _service.GetSubmissionsByQuizAsync(quizId);
            return Ok(new ResponseDto<List<QuizSubmissionResponseDto>>(result, "Lấy danh sách kết quả thành công"));
        }

        /// <summary>
        /// Lấy lịch sử làm quiz của học sinh
        /// </summary>
        [HttpGet("my-submissions")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMySubmissions()
        {
            var studentId = _currentUserService.GetCurrentUserId() ?? 0;
            var result = await _service.GetSubmissionsByStudentAsync(studentId);
            return Ok(new ResponseDto<List<QuizSubmissionResponseDto>>(result, "Lấy lịch sử làm quiz thành công"));
        }
    }
}

