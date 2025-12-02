using System.Text;
using EduShpere.Application;
using EduShpere.Application.Mappings;
using EduShpere.Application.Services;
using EduShpere.Application.Services.ChatService;
using EduShpere.Application.Services.ClassGroupService;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Domain.Models;
using EduShpere.Hubs;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.AIService;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Repositories.Chat;
using EduShpere.Infrastructure.Repositories.Notifications;
using EduShpere.Infrastructure.Repositories.OneTimeLogin;
using EduShpere.Infrastructure.Repositories.StarPoint;
using EduShpere.Infrastructure.Services;
using KidNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace EduShpere
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<EduShpereDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
                options.EnableSensitiveDataLogging(false);
                options.EnableServiceProviderCaching(false);
            });
            builder.Services.AddSingleton<ContentModerationService>(_ =>
            {
                var modelPath = Path.Combine(AppContext.BaseDirectory, "models", "model.zip");
                return new ContentModerationService(modelPath);
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Services.AddSingleton<AppMongoDbContext>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddHostedService<SubmissionScoreUpdateService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
            builder.Services.AddScoped<EduShpere.Infrastructure.Repositories.TeacherProfile.ITeacherProfileRepository, EduShpere.Infrastructure.Repositories.TeacherProfile.TeacherProfileRepository>();
            builder.Services.AddScoped<IOneTimeLoginRepository, OneTimeLoginRepository>();
            builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
            builder.Services.AddScoped<IActivityParticipantRepository, ActivityParticipantRepository>();
            builder.Services.AddScoped<IActivityRuleRepository, ActivityRuleRepository>();
            builder.Services.AddScoped<IActivitySpeakerRepository, ActivitySpeakerRepository>();
            builder.Services.AddScoped<IActivityProgramRepository, ActivityProgramRepository>();
            builder.Services.AddScoped<IActivitySportRepository, ActivitySportRepository>();
            builder.Services.AddScoped<IActivityDetailRepository, ActivityDetailRepository>();
            builder.Services.AddScoped<IActivityRegistrationRewardRepository, ActivityRegistrationRewardRepository>();
            builder.Services.AddScoped<IActivityRewardRepository, ActivityRewardRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IHashTagRepository, HashTagRepository>();
            builder.Services.AddScoped<IUserRightRepository, UserRightRepository>();
            builder.Services.AddScoped<IModerationService, ModerationService>();
            // Search repositories
            builder.Services.AddScoped<EduShpere.Infrastructure.Repositories.SearchHistory.ISearchHistoryRepository, EduShpere.Infrastructure.Repositories.SearchHistory.SearchHistoryRepository>();
            builder.Services.AddScoped<EduShpere.Infrastructure.Repositories.SearchAnalytics.ISearchAnalyticsRepository, EduShpere.Infrastructure.Repositories.SearchAnalytics.SearchAnalyticsRepository>();
            // Search services
            builder.Services.AddScoped<EduShpere.Application.Services.RankingService.IRankingService, EduShpere.Application.Services.RankingService.RankingService>();
            // Background service for trending updates
            builder.Services.AddHostedService<EduShpere.Infrastructure.Services.TrendingUpdateService>();
            builder.Services.AddScoped<ISubmissionReposiory, SubmissionRepository>();

            builder.Services.AddScoped<IClassGroupRepository, ClassGroupRepository>();
            builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            builder.Services.AddScoped<IPostHashTagRepository, PostHashTagRepository>();
            builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
            builder.Services.AddScoped<ICollectionIteamRepository, CollectionIteamRepository>();
            builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            builder.Services.AddScoped<IClubCreationRepository, ClubCreationRequestRepository>();
            builder.Services.AddScoped<IClubRepository, ClubRepository>();
            builder.Services.AddScoped<IClubMemberRepository, ClubMemberRepository>();
            builder.Services.AddScoped<IClubJoinRequestRepository, ClubJoinRequestRepository>();
            builder.Services.AddScoped<IClubCategoryRepository, ClubCategoryRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IRewardRuleRepository, RewardRuleRepository>();
            builder.Services.AddScoped<IRewardRepository, RewardRepository>();
            builder.Services.AddScoped<IRewardRedemptionRepository, RewardRedemptionRepository>();
            builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
            builder.Services.AddScoped<IJuryActivityRepository, JuryActivityRepository>();
            builder.Services.AddScoped<IJuryAssignRepository, JuryAssignRepository>();
            builder.Services.AddScoped<IModerationRepository, ModerationRepository>();


            // Add Configs
            builder.Services.Configure<GoogleAuthConfig>(builder.Configuration.GetSection("GoogleOAuth"));
            builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("Gmail"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddSingleton<CloudinaryService>();
            builder.Services.AddSignalR();
            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.ModelValidatorProviders.Clear();
            })
            .AddJsonOptions(options =>
            {
                // Handle circular references by ignoring cycles
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = false;
            });

            // Register KidNet services
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IPaginationService, PaginationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IActivityService, ActivityService>();
            builder.Services.AddScoped<ITournamentScheduleService, TournamentScheduleService>();
            builder.Services.AddScoped<IHttpContextService, HttpContextService>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<EduShpere.Application.Services.SearchService.ISearchService, EduShpere.Application.Services.SearchService.SearchService>();
            builder.Services.AddScoped<ICollectionService, CollectionService>();
            builder.Services.AddScoped<IClubCreationRequestService, ClubCreationRequestService>();
            builder.Services.AddScoped<IClubService, ClubService>();
            builder.Services.AddScoped<IClubJoinRequestService, ClubJoinRequestService>();
            builder.Services.AddScoped<IClubMemberService, ClubMemberService>(); 
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IRewardRuleService, RewardRuleService>();
            builder.Services.AddScoped<IRewardService, RewardService>();
            builder.Services.AddScoped<IRewardRedemptionService, RewardRedemptionService>();
            builder.Services.AddScoped<IPointHistoryService, PointHistoryService>();
            builder.Services.AddScoped<IStaffService, StaffService>();
            builder.Services.AddScoped<ISubmissionService, SubmissionService>();
            builder.Services.AddScoped<Moderation>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ActivityProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(TeacherProfileMapping).Assembly);
            builder.Services.AddAutoMapper(typeof(PostProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(EduShpere.Application.Mappings.SearchProfile).Assembly);
            builder.Services.AddScoped<IActivityParticipantService, ActivityParticipantService>();
            builder.Services.AddScoped<IStudentImportService, StudentImportService>();
            builder.Services.AddScoped<IClassGroupService, ClassGroupService>();
            builder.Services.AddScoped<IJuryService, JuryService>();

            // System Announcement Service
            builder.Services.AddScoped<EduShpere.Application.Services.SystemAnnouncementService.ISystemAnnouncementService, EduShpere.Application.Services.SystemAnnouncementService.SystemAnnouncementService>();
            
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ActivityParticipantProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(Attachment).Assembly);
            builder.Services.AddAutoMapper(typeof(ClassGroupProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(SystemAnnouncementProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(CollectionProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ClubCreationRequestProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ClubProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ClubJoinRequestProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(CommentProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(StudentImportProfile).Assembly);
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddAutoMapper(typeof(SubmissionProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(ModerationProfile).Assembly);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var secretKey = builder.Configuration["AppSetting:SecretKey"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "EduShpere Swagger API",
                    Description = "An ASP.NET Core Web API for EduShpere Web App",

                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
            });
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var path = context.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/hubs/notification") || path.StartsWithSegments("/hubs/chat"))
                            {
                                var accessToken = context.Request.Query["access_token"];
                                if (string.IsNullOrEmpty(accessToken))
                                {
                                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                                        accessToken = authHeader.Substring("Bearer ".Length).Trim();
                                }
                                if (!string.IsNullOrEmpty(accessToken))
                                    context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };

                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });

                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins) // React dev origin
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseGlobalExceptionHandler();
            app.UseHttpsRedirection();
            
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<NotificationHub>("/hubs/notification");
            app.MapHub<ChatHub>("/hubs/chat");

      
            app.MapControllers();
            app.Run();
        }
    }
}