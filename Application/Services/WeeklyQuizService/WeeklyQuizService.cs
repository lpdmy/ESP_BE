using EduShpere.Application.DTOs.WeeklyQuizDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.WeeklyQuizRepo;
using EduShpere.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.WeeklyQuizService
{
    public class WeeklyQuizService : IWeeklyQuizService
    {
        private readonly IWeeklyQuizRepository _quizRepository;
        private readonly IQuizSubmissionRepository _submissionRepository;
        private readonly IUserRepository _userRepository;

        public WeeklyQuizService(
            IWeeklyQuizRepository quizRepository,
            IQuizSubmissionRepository submissionRepository,
            IUserRepository userRepository)
        {
            _quizRepository = quizRepository;
            _submissionRepository = submissionRepository;
            _userRepository = userRepository;
        }

        public async Task<WeeklyQuizResponseDto> CreateAsync(CreateWeeklyQuizDto dto, int teacherId, string? teacherName = null)
        {
            // Validate deadline
            if (dto.Deadline <= DateTime.UtcNow)
            {
                throw new BadRequestException("Deadline phải sau thời điểm hiện tại");
            }

            // Check duplicate week/year
            if (await _quizRepository.ExistsAsync(dto.WeekNumber, dto.Year))
            {
                throw new BadRequestException($"Đã tồn tại quiz cho tuần {dto.WeekNumber} năm {dto.Year}");
            }

            // Validate questions
            ValidateQuestions(dto.Questions);

            // Get teacher name if not provided
            if (string.IsNullOrEmpty(teacherName))
            {
                var teacher = await _userRepository.GetByIdAsync(teacherId);
                teacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : null;
            }

            // Map to model
            var quiz = new WeeklyQuiz
            {
                Title = dto.Title,
                Description = dto.Description,
                WeekNumber = dto.WeekNumber,
                Year = dto.Year,
                Questions = dto.Questions.Select((q, index) => new QuizQuestion
                {
                    QuestionId = index + 1,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = q.Options?.Select(o => new QuizAnswerOption
                    {
                        Index = o.Index,
                        Text = o.Text
                    }).ToList(),
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points,
                    Order = q.Order
                }).ToList(),
                Deadline = dto.Deadline.ToUniversalTime(),
                TimeLimitMinutes = dto.TimeLimitMinutes,
                MaxScore = dto.MaxScore,
                CreatedByUserId = teacherId,
                CreatedByUserName = teacherName,
                IsActive = true
            };

            var created = await _quizRepository.AddAsync(quiz);
            return MapToResponseDto(created, includeCorrectAnswers: true);
        }

        public async Task<WeeklyQuizResponseDto?> GetByIdAsync(string id, bool includeCorrectAnswers = false)
        {
            var quiz = await _quizRepository.GetByIdAsync(id);
            if (quiz == null) return null;

            return MapToResponseDto(quiz, includeCorrectAnswers);
        }

        public async Task<List<WeeklyQuizResponseDto>> GetByWeekAndYearAsync(int weekNumber, int year)
        {
            var quizzes = await _quizRepository.GetByWeekAndYearAsync(weekNumber, year);
            return quizzes.Select(q => MapToResponseDto(q, includeCorrectAnswers: false)).ToList();
        }

        public async Task<List<WeeklyQuizResponseDto>> GetAllActiveAsync()
        {
            var quizzes = await _quizRepository.GetAllActiveAsync();
            return quizzes.Select(q => MapToResponseDto(q, includeCorrectAnswers: false)).ToList();
        }

        public async Task<List<WeeklyQuizResponseDto>> GetByTeacherAsync(int teacherId)
        {
            var quizzes = await _quizRepository.GetByTeacherAsync(teacherId);
            return quizzes.Select(q => MapToResponseDto(q, includeCorrectAnswers: true)).ToList();
        }

        public async Task<WeeklyQuizResponseDto?> UpdateAsync(UpdateWeeklyQuizDto dto, int teacherId)
        {
            var quiz = await _quizRepository.GetByIdAsync(dto.Id);
            if (quiz == null)
            {
                throw new NotFoundException("Quiz không tồn tại");
            }

            // Check ownership
            if (quiz.CreatedByUserId != teacherId)
            {
                throw new ForbiddenException("Bạn không có quyền sửa quiz này");
            }

            // Check if deadline passed
            if (DateTime.UtcNow > quiz.Deadline)
            {
                throw new BadRequestException("Không thể sửa quiz đã hết hạn");
            }

            // Check duplicate week/year (exclude current quiz)
            if (await _quizRepository.ExistsAsync(dto.WeekNumber, dto.Year, dto.Id))
            {
                throw new BadRequestException($"Đã tồn tại quiz khác cho tuần {dto.WeekNumber} năm {dto.Year}");
            }

            // Validate deadline
            if (dto.Deadline <= DateTime.UtcNow)
            {
                throw new BadRequestException("Deadline phải sau thời điểm hiện tại");
            }

            // Validate questions
            ValidateQuestions(dto.Questions);

            // Update quiz
            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.WeekNumber = dto.WeekNumber;
            quiz.Year = dto.Year;
            quiz.Questions = dto.Questions.Select((q, index) => new QuizQuestion
            {
                QuestionId = index + 1,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options?.Select(o => new QuizAnswerOption
                {
                    Index = o.Index,
                    Text = o.Text
                }).ToList(),
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points,
                Order = q.Order
            }).ToList();
            quiz.Deadline = dto.Deadline.ToUniversalTime();
            quiz.TimeLimitMinutes = dto.TimeLimitMinutes;
            quiz.MaxScore = dto.MaxScore;
            quiz.UpdatedAt = DateTime.UtcNow;

            var updated = await _quizRepository.UpdateAsync(dto.Id, quiz);
            return updated != null ? MapToResponseDto(updated, includeCorrectAnswers: true) : null;
        }

        public async Task<bool> DeleteAsync(string id, int teacherId)
        {
            var quiz = await _quizRepository.GetByIdAsync(id);
            if (quiz == null)
            {
                throw new NotFoundException("Quiz không tồn tại");
            }

            // Check ownership
            if (quiz.CreatedByUserId != teacherId)
            {
                throw new ForbiddenException("Bạn không có quyền xóa quiz này");
            }

            return await _quizRepository.DeleteAsync(id);
        }

        public async Task<QuizSubmissionResponseDto> SubmitQuizAsync(SubmitQuizDto dto, int studentId, string? studentName = null)
        {
            // Get quiz
            var quiz = await _quizRepository.GetByIdAsync(dto.QuizId);
            if (quiz == null)
            {
                throw new NotFoundException("Quiz không tồn tại");
            }

            // Check deadline
            if (DateTime.UtcNow > quiz.Deadline)
            {
                throw new BadRequestException("Quiz đã hết hạn");
            }

            // Check if already submitted
            if (await _submissionRepository.HasStudentSubmittedAsync(dto.QuizId, studentId))
            {
                throw new BadRequestException("Bạn đã làm quiz này rồi");
            }

            // Get student name if not provided
            if (string.IsNullOrEmpty(studentName))
            {
                var student = await _userRepository.GetByIdAsync(studentId);
                studentName = student != null ? $"{student.FirstName} {student.LastName}" : null;
            }

            // Grade answers
            var gradedAnswers = GradeAnswers(quiz, dto.Answers);
            var totalScore = gradedAnswers.Sum(a => a.Points);

            // Create submission
            var submission = new QuizSubmission
            {
                QuizId = dto.QuizId,
                StudentId = studentId,
                StudentName = studentName,
                WeekNumber = quiz.WeekNumber,
                Year = quiz.Year,
                Answers = gradedAnswers,
                Score = totalScore,
                MaxScore = quiz.MaxScore,
                StartedAt = dto.StartedAt.ToUniversalTime(),
                SubmittedAt = DateTime.UtcNow,
                TimeSpentSeconds = dto.TimeSpentSeconds,
                IsGraded = true
            };

            var created = await _submissionRepository.AddAsync(submission);

            return MapSubmissionToResponseDto(created, quiz.Title);
        }

        public async Task<QuizSubmissionResponseDto?> GetSubmissionAsync(string quizId, int studentId)
        {
            var submission = await _submissionRepository.GetByQuizAndStudentAsync(quizId, studentId);
            if (submission == null) return null;

            var quiz = await _quizRepository.GetByIdAsync(quizId);
            return MapSubmissionToResponseDto(submission, quiz?.Title);
        }

        public async Task<List<QuizSubmissionResponseDto>> GetSubmissionsByQuizAsync(string quizId)
        {
            var submissions = await _submissionRepository.GetByQuizAsync(quizId);
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            return submissions.Select(s => MapSubmissionToResponseDto(s, quiz?.Title)).ToList();
        }

        public async Task<List<QuizSubmissionResponseDto>> GetSubmissionsByStudentAsync(int studentId)
        {
            var submissions = await _submissionRepository.GetByStudentAsync(studentId);
            return submissions.Select(s => MapSubmissionToResponseDto(s, null)).ToList();
        }

        public async Task<bool> HasStudentSubmittedAsync(string quizId, int studentId)
        {
            return await _submissionRepository.HasStudentSubmittedAsync(quizId, studentId);
        }

        #region Private Methods

        private void ValidateQuestions(List<CreateQuizQuestionDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                throw new BadRequestException("Quiz phải có ít nhất 1 câu hỏi");
            }

            foreach (var question in questions)
            {
                if (question.QuestionType == "MultipleChoice")
                {
                    if (question.Options == null || question.Options.Count < 2)
                    {
                        throw new BadRequestException("Câu hỏi MultipleChoice phải có ít nhất 2 đáp án");
                    }

                    // Validate correct answer is valid index
                    if (!int.TryParse(question.CorrectAnswer, out int correctIndex) ||
                        correctIndex < 0 || correctIndex >= question.Options.Count)
                    {
                        throw new BadRequestException($"Đáp án đúng không hợp lệ cho câu hỏi MultipleChoice");
                    }
                }
                else if (question.QuestionType == "TrueFalse")
                {
                    if (question.CorrectAnswer.ToLower() != "true" && question.CorrectAnswer.ToLower() != "false")
                    {
                        throw new BadRequestException("Câu hỏi TrueFalse phải có đáp án là 'true' hoặc 'false'");
                    }
                }
                // ShortAnswer không cần validate đặc biệt
            }
        }

        private List<AnswerSubmission> GradeAnswers(WeeklyQuiz quiz, List<AnswerSubmissionDto> studentAnswers)
        {
            var gradedAnswers = new List<AnswerSubmission>();

            foreach (var question in quiz.Questions.OrderBy(q => q.Order))
            {
                var studentAnswer = studentAnswers.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                var answerText = studentAnswer?.Answer ?? "";

                bool isCorrect = false;
                int points = 0;

                if (question.QuestionType == "MultipleChoice")
                {
                    // Compare index
                    if (int.TryParse(answerText, out int studentIndex) &&
                        int.TryParse(question.CorrectAnswer, out int correctIndex))
                    {
                        isCorrect = studentIndex == correctIndex;
                    }
                }
                else if (question.QuestionType == "TrueFalse")
                {
                    // Compare case-insensitive
                    isCorrect = answerText.ToLower().Trim() == question.CorrectAnswer.ToLower().Trim();
                }
                else if (question.QuestionType == "ShortAnswer")
                {
                    // Simple text comparison (case-insensitive, trim)
                    // Có thể cải thiện bằng fuzzy matching hoặc keyword matching
                    isCorrect = answerText.ToLower().Trim() == question.CorrectAnswer.ToLower().Trim();
                }

                points = isCorrect ? question.Points : 0;

                gradedAnswers.Add(new AnswerSubmission
                {
                    QuestionId = question.QuestionId,
                    Answer = answerText,
                    Points = points,
                    IsCorrect = isCorrect
                });
            }

            return gradedAnswers;
        }

        private WeeklyQuizResponseDto MapToResponseDto(WeeklyQuiz quiz, bool includeCorrectAnswers)
        {
            return new WeeklyQuizResponseDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                WeekNumber = quiz.WeekNumber,
                Year = quiz.Year,
                Questions = quiz.Questions.OrderBy(q => q.Order).Select(q => new QuizQuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Options = q.Options?.Select(o => new AnswerOptionResponseDto
                    {
                        Index = o.Index,
                        Text = o.Text
                    }).ToList(),
                    CorrectAnswer = includeCorrectAnswers ? q.CorrectAnswer : null,
                    Points = q.Points,
                    Order = q.Order
                }).ToList(),
                Deadline = quiz.Deadline,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                MaxScore = quiz.MaxScore,
                CreatedByUserId = quiz.CreatedByUserId,
                CreatedByUserName = quiz.CreatedByUserName,
                IsActive = quiz.IsActive,
                CreatedAt = quiz.CreatedAt,
                UpdatedAt = quiz.UpdatedAt
            };
        }

        private QuizSubmissionResponseDto MapSubmissionToResponseDto(QuizSubmission submission, string? quizTitle)
        {
            return new QuizSubmissionResponseDto
            {
                Id = submission.Id,
                QuizId = submission.QuizId,
                QuizTitle = quizTitle,
                StudentId = submission.StudentId,
                StudentName = submission.StudentName,
                WeekNumber = submission.WeekNumber,
                Year = submission.Year,
                Answers = submission.Answers.Select(a => new AnswerSubmissionResponseDto
                {
                    QuestionId = a.QuestionId,
                    Answer = a.Answer,
                    Points = a.Points,
                    IsCorrect = a.IsCorrect,
                    CorrectAnswer = null // Có thể thêm logic để hiển thị đáp án đúng sau khi submit
                }).ToList(),
                Score = submission.Score,
                MaxScore = submission.MaxScore,
                StartedAt = submission.StartedAt,
                SubmittedAt = submission.SubmittedAt,
                TimeSpentSeconds = submission.TimeSpentSeconds,
                IsGraded = submission.IsGraded
            };
        }

        #endregion
    }
}

