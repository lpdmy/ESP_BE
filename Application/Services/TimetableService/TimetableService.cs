using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EduShpere.Application.DTOs.TimetableDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Services;
using EduShpere.Shared.Constants;
using EduShpere.Application;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using EduShpere.Shared;

namespace EduShpere.Application.Services;

public class TimetableService : ITimetableService
{
    private readonly EduShpereDbContext _context;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IAuditService _auditService;

    public TimetableService(
        EduShpereDbContext context,
        IClassGroupRepository classGroupRepository,
        IAuditService auditService)
    {
        _context = context;
        _classGroupRepository = classGroupRepository;
        _auditService = auditService;
    }

    public async Task<TimetableImportResultDto> ImportTimetableAsync(TimetableImportDto dto, int userId)
    {
        var result = new TimetableImportResultDto();
        var errors = new List<string>();

        // Validate file type
        var allowedExtensions = new[] { ".csv", ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(dto.File.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new BadRequestException("Chỉ chấp nhận file CSV hoặc Excel (.csv, .xlsx, .xls)");
        }

        // Validate AcademicYear
        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(ay => ay.Id == dto.AcademicYearId);
        if (academicYear == null)
        {
            throw new NotFoundException("Không tìm thấy niên khóa.");
        }

        // Validate ClassGroup nếu có
        if (dto.ClassGroupId.HasValue)
        {
            var classGroup = await _context.ClassGroups
                .FirstOrDefaultAsync(cg => cg.Id == dto.ClassGroupId.Value && 
                                           cg.AcademicYearId == dto.AcademicYearId && 
                                           !cg.IsDeleted);
            if (classGroup == null)
            {
                throw new NotFoundException("Lớp không tồn tại hoặc không thuộc niên khóa đã chọn.");
            }
        }

        // Xóa dữ liệu cũ nếu ReplaceExisting = true
        if (dto.ReplaceExisting)
        {
            IQueryable<Timetable> queryToDelete = _context.Timetables.Where(t => !t.IsDeleted);
            
            if (dto.ClassGroupId.HasValue)
            {
                queryToDelete = queryToDelete.Where(t => t.ClassGroupId == dto.ClassGroupId.Value);
            }
            else
            {
                // Xóa tất cả timetable của các lớp thuộc niên khóa
                var classGroupIds = await _context.ClassGroups
                    .Where(cg => cg.AcademicYearId == dto.AcademicYearId && !cg.IsDeleted)
                    .Select(cg => cg.Id)
                    .ToListAsync();
                
                queryToDelete = queryToDelete.Where(t => classGroupIds.Contains(t.ClassGroupId));
            }

            var timetablesToDelete = await queryToDelete.ToListAsync();
            foreach (var timetable in timetablesToDelete)
            {
                _auditService.SetAuditFieldsForDelete(timetable);
            }
            await _context.SaveChangesAsync();
        }

        // Parse file
        using var stream = new MemoryStream();
        await dto.File.CopyToAsync(stream);
        stream.Position = 0;

        var timetables = new List<Timetable>();
        int lineNumber = 0;

        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            });

            // Read CSV records
            //await foreach (var record in csv.GetRecordsAsync<dynamic>())
            //{
            //    lineNumber++;
            //    try
            //    {
            //        var classGroupId = dto.ClassGroupId ?? int.Parse(record.ClassGroupId?.ToString() ?? "0");
            //        var dayOfWeek = int.Parse(record.DayOfWeek?.ToString() ?? "0");
            //        var startTimeStr = record.StartTime?.ToString() ?? "";
            //        var endTimeStr = record.EndTime?.ToString() ?? "";
            //        // Validate ClassGroup
            //        var classGroup = await _context.ClassGroups
            //            .FirstOrDefaultAsync(cg => cg.Id == classGroupId && 
            //                                      cg.AcademicYearId == dto.AcademicYearId && 
            //                                      !cg.IsDeleted);
            //        if (classGroup == null)
            //        {
            //            errors.Add($"Line {lineNumber}: Lớp {classGroupId} không tồn tại hoặc không thuộc niên khóa hiện tại.");
            //            result.FailedCount++;
            //            continue;
            //        }

            //        // Validate DayOfWeek
            //        if (dayOfWeek < 1 || dayOfWeek > 7)
            //        {
            //            errors.Add($"Line {lineNumber}: DayOfWeek phải từ 1-7 (1=Thứ 2, 7=Chủ nhật).");
            //            result.FailedCount++;
            //            continue;
            //        }

            //        // Parse time
            //        if (!TimeSpan.TryParse(startTimeStr, out var startTime) ||
            //            !TimeSpan.TryParse(endTimeStr, out var endTime))
            //        {
            //            errors.Add($"Line {lineNumber}: Định dạng thời gian không hợp lệ (StartTime: {startTimeStr}, EndTime: {endTimeStr}).");
            //            result.FailedCount++;
            //            continue;
            //        }

            //        if (endTime <= startTime)
            //        {
            //            errors.Add($"Line {lineNumber}: EndTime phải lớn hơn StartTime.");
            //            result.FailedCount++;
            //            continue;
            //        }

            //        // Check duplicate slot trong cùng lớp
            //        var hasConflict = await _context.Timetables
            //            .AnyAsync(t => t.ClassGroupId == classGroupId &&
            //                          t.DayOfWeek == dayOfWeek &&
            //                          !t.IsDeleted &&
            //                          ((t.StartTime < endTime && t.EndTime > startTime)));
                    
            //        if (hasConflict)
            //        {
            //            errors.Add($"Line {lineNumber}: Trùng slot với lịch học đã có (Lớp {classGroupId}, Thứ {dayOfWeek}, {startTime:hh\\:mm}-{endTime:hh\\:mm}).");
            //            result.FailedCount++;
            //            continue;
            //        }

            //        // Create Timetable
            //        var timetable = new Timetable
            //        {
            //            ClassGroupId = classGroupId,
            //            DayOfWeek = dayOfWeek,
            //            StartTime = startTime,
            //            EndTime = endTime,
            //            SubjectName = record.SubjectName?.ToString(),
            //            Location = record.Location?.ToString()
            //        };

            //        _auditService.SetAuditFieldsForCreate(timetable);
            //        timetables.Add(timetable);
            //    }
            //    catch (Exception ex)
            //    {
            //        errors.Add($"Line {lineNumber}: Lỗi xử lý - {ex.Message}");
            //        result.FailedCount++;
            //    }
            //}

            // Save to database
            if (timetables.Any())
            {
                await _context.Timetables.AddRangeAsync(timetables);
                await _context.SaveChangesAsync();
                result.ImportedCount = timetables.Count;
            }

            result.Errors = errors;
            result.Message = $"Import thành công {result.ImportedCount} bản ghi. {result.FailedCount} bản ghi lỗi.";
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Lỗi khi import file: {ex.Message}");
        }

        return result;
    }

    public async Task<List<TimetableDto>> GetTimetablesByClassGroupAsync(int classGroupId)
    {
        var timetables = await _context.Timetables
            .Include(t => t.ClassGroup)
            .Where(t => t.ClassGroupId == classGroupId && !t.IsDeleted)
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

        return timetables.Select(t => new TimetableDto
        {
            Id = t.Id,
            ClassGroupId = t.ClassGroupId,
            ClassGroupName = t.ClassGroup?.Name,
            DayOfWeek = t.DayOfWeek,
            StartTime = t.StartTime,
            EndTime = t.EndTime,
            SubjectName = t.SubjectName,
            Location = t.Location,
            CreatedAt = (DateTime) t.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteTimetablesByClassGroupAsync(int classGroupId, int userId)
    {
        var timetables = await _context.Timetables
            .Where(t => t.ClassGroupId == classGroupId && !t.IsDeleted)
            .ToListAsync();

        foreach (var timetable in timetables)
        {
            _auditService.SetAuditFieldsForDelete(timetable);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}


