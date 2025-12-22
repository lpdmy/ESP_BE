using EduShpere.Application.DTOs.StatisticsReportDto;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduShpere.Application.Services.StatisticsReportService;

public class StatisticsReportExportService : IStatisticsReportExportService
{
    public Task<byte[]> ExportToPdfAsync(
        AcademicYearStatisticsReport report,
        StatisticsReportRequest request)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var stream = new MemoryStream();
            
            var document = Document.Create(container =>
            {
                // Page 1: Cover Page
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(56);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                    
                    page.Content()
                        .PaddingVertical(56)
                        .Column(column =>
                        {
                            column.Item().Element(ComposeCoverPage(report));
                        });
                });
                
                // Page 2: Table of Contents
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(56);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                    
                    page.Header()
                        .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                        .Bold().FontSize(14).FontColor(Colors.Black)
                        .AlignCenter();
                    
                    page.Content()
                        .PaddingVertical(28)
                        .Column(column =>
                        {
                            column.Item().Element(ComposeTableOfContents(report, request));
                        });
                    
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
                
                // Page 3: Overview Section
                if (report.Overview != null)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                        
                        page.Header()
                            .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                            .Bold().FontSize(14).FontColor(Colors.Black)
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28)
                            .Column(column =>
                            {
                                column.Item().Element(ComposeOverviewSection(report.Overview));
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                }
                
                // Page 3: Class Table
                if (request.IncludeClassRewardTable && report.Classes != null && report.Classes.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                        
                        page.Header()
                            .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                            .Bold().FontSize(14).FontColor(Colors.Black)
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28)
                            .Column(column =>
                            {
                                column.Item().Element(ComposeClassTable(report.Classes));
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                }
                
                // Page 4: Student Table
                if (request.IncludeStudentRewardTable && report.Students != null && report.Students.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                        
                        page.Header()
                            .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                            .Bold().FontSize(14).FontColor(Colors.Black)
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28)
                            .Column(column =>
                            {
                                column.Item().Element(ComposeStudentTable(report.Students));
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                }
                
                // Page 5: Weekly Trends
                if (request.IncludeWeeklyTrendCharts && report.WeeklyTrends != null && report.WeeklyTrends.Weeks.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                        
                        page.Header()
                            .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                            .Bold().FontSize(14).FontColor(Colors.Black)
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28)
                            .Column(column =>
                            {
                                column.Item().Element(ComposeWeeklyTrends(report.WeeklyTrends));
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                }
                
                // Page 6: Activity Type Statistics
                if (report.ActivityTypeStatistics != null && report.ActivityTypeStatistics.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                        
                        page.Header()
                            .Text($"BÁO CÁO THỐNG KÊ NĂM HỌC {report.AcademicYear}")
                            .Bold().FontSize(14).FontColor(Colors.Black)
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28)
                            .Column(column =>
                            {
                                column.Item().Element(ComposeActivityTypeStatistics(report.ActivityTypeStatistics));
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                }
                
                // Last Page: Conclusion Page
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(56);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial Unicode MS"));
                    
                    page.Content()
                        .PaddingVertical(56)
                        .Column(column =>
                        {
                            column.Item().Element(ComposeConclusionPage(report));
                        });
                });
            });
        
            document.GeneratePdf(stream);
            stream.Position = 0; // Reset stream position to beginning
            var pdfBytes = stream.ToArray();
            stream.Close();
            stream.Dispose();
            
            // Verify PDF bytes (PDF files start with %PDF)
            if (pdfBytes.Length == 0 || (pdfBytes.Length >= 4 && System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4) != "%PDF"))
            {
                throw new InvalidOperationException("PDF generation failed: Invalid PDF format");
            }
            
            return Task.FromResult(pdfBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tạo PDF: {ex.Message}", ex);
        }
    }
    
    private Action<IContainer> ComposeCoverPage(AcademicYearStatisticsReport report)
    {
        return container =>
        {
            container
                .Column(column =>
                {
                    // Top spacing
                    column.Item().Height(80);
                    
                    // Banner/Header with orange background
                    column.Item()
                        .Background(Colors.Orange.Lighten4)
                        .Padding(40)
                        .Column(bannerColumn =>
                        {
                            bannerColumn.Item()
                                .AlignCenter()
                                .Text("📊")
                                .FontSize(48)
                                .FontColor(Colors.Orange.Darken2);
                            
                            bannerColumn.Item().Height(20);
                            
                            bannerColumn.Item()
                                .AlignCenter()
                                .Text("BÁO CÁO THỐNG KÊ")
                                .Bold()
                                .FontSize(28)
                                .FontColor(Colors.Grey.Darken4)
                                .LineHeight(1.2f);
                            
                            bannerColumn.Item().Height(8);
                            
                            bannerColumn.Item()
                                .AlignCenter()
                                .Text($"NĂM HỌC {report.AcademicYear}")
                                .Bold()
                                .FontSize(24)
                                .FontColor(Colors.Orange.Darken2)
                                .LineHeight(1.2f);
                            
                            bannerColumn.Item().Height(16);
                            
                            bannerColumn.Item()
                                .AlignCenter()
                                .Text("Hệ thống quản lý hoạt động & điểm số học sinh")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken2)
                                .Italic();
                        });
                    
                    column.Item().Height(60);
                    
                    // Summary Box
                    if (report.Overview != null)
                    {
                        column.Item()
                            .Border(2)
                            .BorderColor(Colors.Orange.Lighten2)
                            .Padding(30)
                            .Column(summaryColumn =>
                            {
                                summaryColumn.Item()
                                    .AlignCenter()
                                    .Text("––––––––––––––––––")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Medium);
                                
                                summaryColumn.Item().Height(12);
                                
                                summaryColumn.Item()
                                    .Row(row =>
                                    {
                                        row.AutoItem()
                                            .Text("• Tổng số lớp: ")
                                            .FontSize(12)
                                            .FontColor(Colors.Grey.Darken3);
                                        row.AutoItem()
                                            .Text(report.Overview.TotalClasses.ToString())
                                            .Bold()
                                            .FontSize(12)
                                            .FontColor(Colors.Orange.Darken2);
                                    });
                                
                                summaryColumn.Item().Height(8);
                                
                                summaryColumn.Item()
                                    .Row(row =>
                                    {
                                        row.AutoItem()
                                            .Text("• Tổng số học sinh: ")
                                            .FontSize(12)
                                            .FontColor(Colors.Grey.Darken3);
                                        row.AutoItem()
                                            .Text(report.Overview.TotalStudents.ToString())
                                            .Bold()
                                            .FontSize(12)
                                            .FontColor(Colors.Orange.Darken2);
                                    });
                                
                                summaryColumn.Item().Height(8);
                                
                                var totalActivities = (report.Classes?.Sum(c => c.ActivityCountInYear) ?? 0) +
                                                     (report.Students?.Sum(s => s.ActivityCountInYear) ?? 0);
                                if (totalActivities > 0)
                                {
                                    summaryColumn.Item()
                                        .Row(row =>
                                        {
                                            row.AutoItem()
                                                .Text("• Tổng hoạt động: ")
                                                .FontSize(12)
                                                .FontColor(Colors.Grey.Darken3);
                                            row.AutoItem()
                                                .Text(totalActivities.ToString())
                                                .Bold()
                                                .FontSize(12)
                                                .FontColor(Colors.Orange.Darken2);
                                        });
                                }
                                
                                summaryColumn.Item().Height(12);
                                
                                summaryColumn.Item()
                                    .AlignCenter()
                                    .Text("––––––––––––––––––")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Medium);
                            });
                    }
                    
                    // Spacer
                    column.Item().Height(80);
                    
                    // Footer info
                    column.Item()
                        .Column(footerColumn =>
                        {
                            footerColumn.Item()
                                .Row(row =>
                                {
                                    row.AutoItem()
                                        .Text("Đơn vị thực hiện: ")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken2);
                                    row.RelativeItem()
                                        .Text("Trường THPT FPT")
                                        .Bold()
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken3);
                                });
                            
                            footerColumn.Item().Height(12);
                            
                            footerColumn.Item()
                                .Row(row =>
                                {
                                    row.AutoItem()
                                        .Text("Thời gian xuất báo cáo: ")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken2);
                                    row.RelativeItem()
                                        .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture))
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken3);
                                });
                        });
                });
        };
    }
    
    private Action<IContainer> ComposeConclusionPage(AcademicYearStatisticsReport report)
    {
        return container =>
        {
            container
                .Column(column =>
                {
                    // Top spacing
                    column.Item().Height(100);
                    
                    // Title
                    column.Item()
                        .AlignCenter()
                        .Text("KẾT LUẬN & GHI CHÚ")
                        .Bold()
                        .FontSize(18)
                        .FontColor(Colors.Grey.Darken4)
                        .LineHeight(1.3f);
                    
                    column.Item().Height(40);
                    
                    // Content box
                    column.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(30)
                        .Column(contentColumn =>
                        {
                            contentColumn.Item()
                                .Text("Báo cáo được tổng hợp tự động từ hệ thống EduSphere")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken3)
                                .LineHeight(1.6f);
                            
                            contentColumn.Item().Height(16);
                            
                            contentColumn.Item()
                                .Text("Dữ liệu có thể thay đổi theo thời gian cập nhật trong hệ thống")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken3)
                                .LineHeight(1.6f);
                            
                            contentColumn.Item().Height(16);
                            
                            contentColumn.Item()
                                .Text("Mọi thắc mắc vui lòng liên hệ:")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken3)
                                .LineHeight(1.6f);
                            
                            contentColumn.Item().Height(8);
                            
                            contentColumn.Item()
                                .PaddingLeft(20)
                                .Text("• Email: support@edusphere.fpt.edu.vn")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2)
                                .LineHeight(1.6f);
                            
                            contentColumn.Item().Height(4);
                            
                            contentColumn.Item()
                                .PaddingLeft(20)
                                .Text("• Hệ thống: EduSphere - Quản lý hoạt động & điểm số học sinh")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2)
                                .LineHeight(1.6f);
                        });
                    
                    // Spacer
                    column.Item().Height(80);
                    
                    // End message
                    column.Item()
                        .AlignCenter()
                        .Column(endColumn =>
                        {
                            endColumn.Item()
                                .Text("––––––––––––––––––")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                            
                            endColumn.Item().Height(16);
                            
                            endColumn.Item()
                                .Text("HẾT BÁO CÁO")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken3);
                            
                            endColumn.Item().Height(8);
                            
                            endColumn.Item()
                                .Text("Cảm ơn quý thầy cô đã theo dõi")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2)
                                .Italic();
                            
                            endColumn.Item().Height(12);
                            
                            endColumn.Item()
                                .Text("––––––––––––––––––")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });
                });
        };
    }
    
    private Action<IContainer> ComposeOverviewSection(SchoolOverviewMetrics overview)
    {
        return container =>
        {
            container
                .PaddingVertical(10)
                .Column(column =>
                {
                    column.Item().Text("TỔNG QUAN").Bold().FontSize(12);
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });
                        
                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(5).Text("Chỉ tiêu").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text("Giá trị").Bold().FontSize(9);
                        });
                        
                        // Rows
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Tổng số lớp").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.TotalClasses.ToString("N0")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Tổng số học sinh").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.TotalStudents.ToString("N0")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Tổng điểm toàn trường").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.TotalSchoolScore.ToString("N0")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Trung bình điểm/lớp").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.AverageScorePerClass.ToString("N2")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Trung bình điểm/học sinh").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.AverageScorePerStudent.ToString("N2")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Số hoạt động trong năm").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.TotalActivitiesInYear.ToString("N0")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Lớp không có điểm").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.ClassesWithZeroScore.ToString("N0")).FontSize(9);
                        
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).Text("Học sinh không có điểm").FontSize(9);
                        table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(overview.StudentsWithZeroScore.ToString("N0")).FontSize(9);
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeClassTable(IReadOnlyList<ClassYearScoreSummary> classes)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("BẢNG ĐIỂM THEO LỚP").Bold().FontSize(12);
                    column.Item().PaddingTop(2).Text("(Dữ liệu này được sử dụng để vẽ biểu đồ phân bố điểm theo lớp)").Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(0.8f);
                        });
                        
                        // Header - Bold, light background, borders
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Hạng").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Lớp").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("GVCN").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("Số HS").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("TB/HS").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("% tổng").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("Số HĐ").Bold().FontSize(9);
                        });
                        
                        // Rows - Highlight top 5 classes by score (rank 1-5)
                        foreach (var cls in classes)
                        {
                            var isTop5 = cls.RankByScore <= 5;
                            var bgColor = isTop5 ? Colors.Orange.Lighten4 : Colors.White;
                            
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).Text(cls.RankByScore.ToString()).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).Text(cls.ClassCode ?? "").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).Text(cls.HomeroomTeacherName ?? "").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(cls.StudentCount.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(cls.TotalScoreInYear.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(cls.AverageScorePerStudent.ToString("N2")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(cls.ScoreShareOfSchoolTotal.ToString("N2") + "%").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(cls.ActivityCountInYear.ToString("N0")).FontSize(9);
                        }
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeStudentTable(IReadOnlyList<StudentYearScoreSummary> students)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("BẢNG ĐIỂM THEO HỌC SINH").Bold().FontSize(12);
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                        });
                        
                        // Header - Bold, light background, borders
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Mã HS").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Họ tên").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Lớp").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("Số HĐ").Bold().FontSize(9);
                        });
                        
                        // Rows (limit to 1000 rows to avoid PDF size issues)
                        var studentsToShow = students.Take(1000).ToList();
                        foreach (var student in studentsToShow)
                        {
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(student.StudentCode ?? "").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(student.StudentFullName ?? "").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(student.ClassCode ?? "").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(student.TotalScoreInYear.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(student.ActivityCountInYear.ToString("N0")).FontSize(9);
                        }
                        
                        if (students.Count > 1000)
                        {
                            table.Cell().ColumnSpan(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text($"... và {students.Count - 1000} học sinh khác").Italic().FontSize(9);
                        }
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeWeeklyTrends(WeeklyTrendMetrics trends)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("XU HƯỚNG THEO TUẦN").Bold().FontSize(12);
                    column.Item().PaddingTop(2).Text("(Dữ liệu này được sử dụng để vẽ biểu đồ xu hướng hoạt động theo tuần)").Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                        });
                        
                        // Header - Bold, light background, borders
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Tuần").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Ngày bắt đầu").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Ngày kết thúc").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("Số HĐ").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("HS tham gia").Bold().FontSize(9);
                        });
                        
                        // Rows - Simple borders, no background colors
                        foreach (var week in trends.Weeks)
                        {
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(week.WeekIndex.ToString()).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(week.WeekStartDate.ToString("dd/MM/yyyy")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(week.WeekEndDate.ToString("dd/MM/yyyy")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(week.ActivityCount.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(week.TotalScoreInWeek.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(week.ParticipatedStudentCount.ToString("N0")).FontSize(9);
                        }
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeTableOfContents(AcademicYearStatisticsReport report, StatisticsReportRequest request)
    {
        return container =>
        {
            container
                .PaddingBottom(20)
                .Column(column =>
                {
                    column.Item().Text("MỤC LỤC").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(10);
                    
                    var items = new List<(string Title, int PageNumber)>();
                    int currentPage = 3; // Start from page 3 (Cover=1, TOC=2, Content starts at 3)
                    
                    if (report.Overview != null)
                    {
                        items.Add(("1. Tổng quan", currentPage));
                        currentPage++;
                    }
                    
                    if (request.IncludeClassRewardTable && report.Classes != null && report.Classes.Count > 0)
                    {
                        items.Add(("2. Bảng điểm theo lớp", currentPage));
                        currentPage++;
                    }
                    
                    if (request.IncludeStudentRewardTable && report.Students != null && report.Students.Count > 0)
                    {
                        items.Add(("3. Bảng điểm theo học sinh", currentPage));
                        currentPage++;
                    }
                    
                    if (request.IncludeWeeklyTrendCharts && report.WeeklyTrends != null && report.WeeklyTrends.Weeks.Count > 0)
                    {
                        items.Add(("4. Xu hướng hoạt động theo tuần", currentPage));
                        currentPage++;
                    }
                    
                    if (report.ActivityTypeStatistics != null && report.ActivityTypeStatistics.Count > 0)
                    {
                        items.Add(("5. Thống kê loại hoạt động", currentPage));
                        currentPage++;
                    }
                    
                    if (items.Count == 0)
                    {
                        column.Item().Text("Không có nội dung").FontSize(10).FontColor(Colors.Grey.Medium);
                        return;
                    }
                    
                    foreach (var item in items)
                    {
                        column.Item()
                            .PaddingVertical(4)
                            .Row(row =>
                            {
                                row.AutoItem().Text(item.Title).FontSize(10);
                                row.RelativeItem()
                                    .PaddingHorizontal(5)
                                    .AlignCenter()
                                    .Text(new string('.', 30))
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Medium);
                                row.AutoItem().Text(item.PageNumber.ToString()).FontSize(10);
                            });
                    }
                });
        };
    }
    
    private Action<IContainer> ComposeClassDistributionChart(IReadOnlyList<ClassYearScoreSummary> classes)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("BIỂU ĐỒ PHÂN BỐ ĐIỂM THEO LỚP").Bold().FontSize(12);
                    column.Item().PaddingTop(5);
                    
                    if (classes.Count == 0)
                    {
                        column.Item().Text("Không có dữ liệu").FontSize(9).FontColor(Colors.Grey.Lighten1);
                        return;
                    }
                    
                    // Create a simple table representation of top classes
                    var topClasses = classes.OrderByDescending(c => c.TotalScoreInYear).Take(Math.Min(15, classes.Count)).ToList();
                    
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.5f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(1.5f);
                        });
                        
                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).Text("Hạng").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).Text("Lớp").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(8);
                        });
                        
                        // Rows with visual bar representation
                        foreach (var cls in topClasses)
                        {
                            var isTop3 = topClasses.IndexOf(cls) < 3;
                            var bgColor = isTop3 ? Colors.Orange.Lighten4 : Colors.White;
                            
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(2).PaddingHorizontal(3).Text((topClasses.IndexOf(cls) + 1).ToString()).FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(2).PaddingHorizontal(3).Text(cls.ClassCode ?? "").FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(bgColor).PaddingVertical(2).PaddingHorizontal(3).AlignRight().Text(cls.TotalScoreInYear.ToString("N0")).FontSize(8);
                        }
                    });
                    
                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Width(12).Height(12).Background(Colors.Orange.Darken2);
                            r.AutoItem().PaddingLeft(5).Text("Top 5 lớp").FontSize(9);
                        });
                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Width(12).Height(12).Background(Colors.Blue.Medium);
                            r.AutoItem().PaddingLeft(5).Text("Các lớp khác").FontSize(9);
                        });
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeWeeklyTrendChart(WeeklyTrendMetrics trends)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("BIỂU ĐỒ XU HƯỚNG HOẠT ĐỘNG THEO TUẦN").Bold().FontSize(12);
                    column.Item().PaddingTop(5);
                    
                    if (trends.Weeks.Count == 0)
                    {
                        column.Item().Text("Không có dữ liệu").FontSize(9).FontColor(Colors.Grey.Lighten1);
                        return;
                    }
                    
                    // Create a simple table representation showing trends with color coding
                    var sortedWeeks = trends.Weeks.OrderBy(w => w.WeekIndex).ToList();
                    var maxActivity = sortedWeeks.Max(w => w.ActivityCount);
                    var maxScore = sortedWeeks.Max(w => w.TotalScoreInWeek);
                    
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.2f);
                        });
                        
                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).Text("Tuần").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).Text("Ngày bắt đầu").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(3).Text("Ngày kết thúc").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Blue.Lighten4).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text("Số HĐ").Bold().FontSize(8);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Red.Lighten4).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(8);
                        });
                        
                        // Rows with color intensity based on values
                        foreach (var week in sortedWeeks)
                        {
                            var activityIntensity = maxActivity > 0 ? (float)(week.ActivityCount / maxActivity) : 0f;
                            var scoreIntensity = maxScore > 0 ? (float)(week.TotalScoreInWeek / maxScore) : 0f;
                            
                            var activityBg = activityIntensity > 0.7f ? Colors.Blue.Lighten2 : 
                                           activityIntensity > 0.4f ? Colors.Blue.Lighten3 : Colors.Blue.Lighten5;
                            var scoreBg = scoreIntensity > 0.7f ? Colors.Red.Lighten2 : 
                                         scoreIntensity > 0.4f ? Colors.Red.Lighten3 : Colors.Red.Lighten5;
                            
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(2).PaddingHorizontal(3).Text(week.WeekIndex.ToString()).FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(2).PaddingHorizontal(3).Text(week.WeekStartDate.ToString("dd/MM/yyyy")).FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(2).PaddingHorizontal(3).Text(week.WeekEndDate.ToString("dd/MM/yyyy")).FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(activityBg).PaddingVertical(2).PaddingHorizontal(3).AlignCenter().Text(week.ActivityCount.ToString("N0")).FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Background(scoreBg).PaddingVertical(2).PaddingHorizontal(3).AlignRight().Text(week.TotalScoreInWeek.ToString("N0")).FontSize(8);
                        }
                    });
                    
                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Width(12).Height(12).Background(Colors.Blue.Medium);
                            r.AutoItem().PaddingLeft(5).Text("Số hoạt động").FontSize(9);
                        });
                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Width(12).Height(12).Background(Colors.Red.Medium);
                            r.AutoItem().PaddingLeft(5).Text("Tổng điểm").FontSize(9);
                        });
                    });
                });
        };
    }
    
    private Action<IContainer> ComposeActivityTypeStatistics(IReadOnlyList<ActivityTypeStatistics> activityTypes)
    {
        return container =>
        {
            container
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item().Text("THỐNG KÊ LOẠI HOẠT ĐỘNG").Bold().FontSize(12);
                    column.Item().PaddingTop(5);
                    
                    if (activityTypes.Count == 0)
                    {
                        column.Item().Text("Không có dữ liệu").FontSize(9).FontColor(Colors.Grey.Lighten1);
                        return;
                    }
                    
                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2f);
                        });
                        
                        // Header - Only show SubType column (which contains Vietnamese name)
                        table.Header(header =>
                        {
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).Text("Loại hoạt động").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("Số lượng").Bold().FontSize(9);
                            header.Cell().Border(0.5f).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(3).AlignRight().Text("Tổng điểm").Bold().FontSize(9);
                        });
                        
                        // Rows - Only show SubType (Vietnamese name)
                        foreach (var activityType in activityTypes)
                        {
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).Text(activityType.SubType ?? "Không xác định").FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignCenter().Text(activityType.Count.ToString("N0")).FontSize(9);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(3).AlignRight().Text(activityType.TotalScore.ToString("N0")).FontSize(9);
                        }
                    });
                });
        };
    }

    public async Task<byte[]> ExportToExcelAsync(
        AcademicYearStatisticsReport report,
        StatisticsReportRequest request)
    {
        using var workbook = new XLWorkbook();
        
        // Sheet 1: Tổng quan
        var overviewSheet = workbook.Worksheets.Add("Tổng quan");
        
        // Header row
        overviewSheet.Cell(1, 1).Value = "Chỉ tiêu";
        overviewSheet.Cell(1, 2).Value = "Giá trị";
        overviewSheet.Cell(1, 3).Value = "Ghi chú";
        
        var overviewHeaderRange = overviewSheet.Range(1, 1, 1, 3);
        overviewHeaderRange.Style.Font.Bold = true;
        overviewHeaderRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
        overviewHeaderRange.Style.Font.FontColor = XLColor.White;
        overviewHeaderRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        overviewHeaderRange.Style.Border.OutsideBorderColor = XLColor.Black;
        
        if (report.Overview != null)
        {
            int row = 2;
            
            overviewSheet.Cell(row, 1).Value = "Năm học";
            overviewSheet.Cell(row, 2).Value = report.AcademicYear;
            overviewSheet.Cell(row, 3).Value = "Năm học";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Tổng số lớp";
            overviewSheet.Cell(row, 2).Value = report.Overview.TotalClasses;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Tổng số lớp";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Tổng số học sinh";
            overviewSheet.Cell(row, 2).Value = report.Overview.TotalStudents;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Tổng số học sinh";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Tổng điểm toàn trường";
            overviewSheet.Cell(row, 2).Value = report.Overview.TotalSchoolScore;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Tổng điểm toàn trường";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Trung bình điểm/lớp";
            overviewSheet.Cell(row, 2).Value = report.Overview.AverageScorePerClass;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            overviewSheet.Cell(row, 3).Value = "Trung bình điểm/lớp";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Trung bình điểm/học sinh";
            overviewSheet.Cell(row, 2).Value = report.Overview.AverageScorePerStudent;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            overviewSheet.Cell(row, 3).Value = "Trung bình điểm/học sinh";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Số hoạt động trong năm";
            overviewSheet.Cell(row, 2).Value = report.Overview.TotalActivitiesInYear;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Số hoạt động trong năm";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Lớp không có điểm";
            overviewSheet.Cell(row, 2).Value = report.Overview.ClassesWithZeroScore;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Lớp không có điểm";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "Học sinh không có điểm";
            overviewSheet.Cell(row, 2).Value = report.Overview.StudentsWithZeroScore;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "Học sinh không có điểm";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "% lớp top";
            overviewSheet.Cell(row, 2).Value = report.Overview.TopClassPercentageThreshold;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            overviewSheet.Cell(row, 3).Value = "% lớp top";
            row++;
            
            overviewSheet.Cell(row, 1).Value = "% điểm của lớp top";
            overviewSheet.Cell(row, 2).Value = report.Overview.TopClassesScoreSharePercentage;
            overviewSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            overviewSheet.Cell(row, 3).Value = "% điểm của lớp top";
            
            // Enable AutoFilter và thêm màu sắc
            var dataRange = overviewSheet.Range(1, 1, row, 3);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Black;
            dataRange.Style.Border.InsideBorderColor = XLColor.Gray;
            
            // Zebra striping cho dữ liệu
            for (int i = 2; i <= row; i++)
            {
                if (i % 2 == 0)
                {
                    overviewSheet.Range(i, 1, i, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                }
            }
            
            overviewSheet.Range(1, 1, row, 3).SetAutoFilter();
            
            overviewSheet.Columns().AdjustToContents();
        }
        
        // Sheet 2: Điểm theo lớp
        if (request.IncludeClassRewardTable && report.Classes != null && report.Classes.Count > 0)
        {
            var classSheet = workbook.Worksheets.Add("Điểm theo lớp");
            
            // Header row - KHÔNG merge cell
            classSheet.Cell(1, 1).Value = "Năm học";
            classSheet.Cell(1, 2).Value = "Mã lớp";
            classSheet.Cell(1, 3).Value = "Giáo viên chủ nhiệm";
            classSheet.Cell(1, 4).Value = "Số học sinh";
            classSheet.Cell(1, 5).Value = "Số hoạt động";
            classSheet.Cell(1, 6).Value = "Tổng điểm";
            classSheet.Cell(1, 7).Value = "Điểm TB/học sinh";
            classSheet.Cell(1, 8).Value = "Hạng";
            classSheet.Cell(1, 9).Value = "Phần trăm hạng";
            classSheet.Cell(1, 10).Value = "% tổng điểm";
            
            var headerRange = classSheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.OutsideBorderColor = XLColor.Black;
            
            int row = 2;
            foreach (var cls in report.Classes)
            {
                var isTop5 = cls.RankByScore <= 5;
                
                classSheet.Cell(row, 1).Value = report.AcademicYear;
                classSheet.Cell(row, 2).Value = cls.ClassCode;
                classSheet.Cell(row, 3).Value = cls.HomeroomTeacherName ?? "";
                classSheet.Cell(row, 4).Value = cls.StudentCount;
                classSheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                classSheet.Cell(row, 5).Value = cls.ActivityCountInYear;
                classSheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                classSheet.Cell(row, 6).Value = cls.TotalScoreInYear;
                classSheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                classSheet.Cell(row, 7).Value = cls.AverageScorePerStudent;
                classSheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                classSheet.Cell(row, 8).Value = cls.RankByScore;
                classSheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                classSheet.Cell(row, 9).Value = cls.PercentileByScore;
                classSheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
                classSheet.Cell(row, 10).Value = cls.ScoreShareOfSchoolTotal;
                classSheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
                
                // Highlight top 5 classes
                if (isTop5)
                {
                    classSheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFD700"); // Gold
                }
                
                row++;
            }
            
            // Enable AutoFilter và format borders
            var dataRange = classSheet.Range(1, 1, row - 1, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Black;
            dataRange.Style.Border.InsideBorderColor = XLColor.Gray;
            
            // Zebra striping (only for non-highlighted rows)
            for (int i = 2; i < row; i++)
            {
                var rank = classSheet.Cell(i, 8).GetValue<int>();
                var isTop5 = rank <= 5;
                
                // Only apply zebra striping if not top 5
                if (i % 2 == 0 && !isTop5)
                {
                    classSheet.Range(i, 1, i, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                }
            }
            
            classSheet.Range(1, 1, row - 1, 10).SetAutoFilter();
            
            classSheet.Columns().AdjustToContents();
        }
        
        // Sheet 3: Điểm theo học sinh
        if (request.IncludeStudentRewardTable && report.Students != null && report.Students.Count > 0)
        {
            var studentSheet = workbook.Worksheets.Add("Điểm theo học sinh");
            
            // Header row - KHÔNG merge cell
            studentSheet.Cell(1, 1).Value = "Năm học";
            studentSheet.Cell(1, 2).Value = "Mã học sinh";
            studentSheet.Cell(1, 3).Value = "Họ và tên";
            studentSheet.Cell(1, 4).Value = "Lớp";
            studentSheet.Cell(1, 5).Value = "Tổng điểm";
            studentSheet.Cell(1, 6).Value = "Số hoạt động";
            
            var headerRange = studentSheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.OutsideBorderColor = XLColor.Black;
            
            // Get top 5 students by activity count
            var top5StudentsByActivity = report.Students
                .OrderByDescending(s => s.ActivityCountInYear)
                .Take(5)
                .Select(s => s.StudentId)
                .ToHashSet();
            
            int row = 2;
            foreach (var student in report.Students)
            {
                var isTop5 = top5StudentsByActivity.Contains(student.StudentId);
                
                studentSheet.Cell(row, 1).Value = report.AcademicYear;
                studentSheet.Cell(row, 2).Value = student.StudentCode ?? "";
                studentSheet.Cell(row, 3).Value = student.StudentFullName ?? "";
                studentSheet.Cell(row, 4).Value = student.ClassCode ?? "";
                studentSheet.Cell(row, 5).Value = student.TotalScoreInYear;
                studentSheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                studentSheet.Cell(row, 6).Value = student.ActivityCountInYear;
                studentSheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                
                // Highlight top 5 students by activity count
                if (isTop5)
                {
                    studentSheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#90EE90"); // Light green
                }
                
                row++;
            }
            
            // Enable AutoFilter và format borders
            var dataRange = studentSheet.Range(1, 1, row - 1, 6);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Black;
            dataRange.Style.Border.InsideBorderColor = XLColor.Gray;
            
            // Zebra striping (only for non-highlighted rows)
            int studentIndex = 0;
            for (int i = 2; i < row; i++)
            {
                if (studentIndex < report.Students.Count)
                {
                    var student = report.Students[studentIndex];
                    var isTop5 = top5StudentsByActivity.Contains(student.StudentId);
                    
                    // Only apply zebra striping if not top 5
                    if (i % 2 == 0 && !isTop5)
                    {
                        studentSheet.Range(i, 1, i, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                    }
                }
                studentIndex++;
            }
            
            studentSheet.Range(1, 1, row - 1, 6).SetAutoFilter();
            
            studentSheet.Columns().AdjustToContents();
        }
        
        // Sheet 4: Xu hướng theo tuần
        if (request.IncludeWeeklyTrendCharts && report.WeeklyTrends != null && report.WeeklyTrends.Weeks.Count > 0)
        {
            var trendSheet = workbook.Worksheets.Add("Xu hướng theo tuần");
            
            // Header row - KHÔNG merge cell
            trendSheet.Cell(1, 1).Value = "Năm học";
            trendSheet.Cell(1, 2).Value = "Tuần";
            trendSheet.Cell(1, 3).Value = "Ngày bắt đầu";
            trendSheet.Cell(1, 4).Value = "Ngày kết thúc";
            trendSheet.Cell(1, 5).Value = "Số hoạt động";
            trendSheet.Cell(1, 6).Value = "Tổng điểm";
            trendSheet.Cell(1, 7).Value = "Số học sinh tham gia";
            
            var headerRange = trendSheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.OutsideBorderColor = XLColor.Black;
            
            int row = 2;
            foreach (var week in report.WeeklyTrends.Weeks)
            {
                trendSheet.Cell(row, 1).Value = report.AcademicYear;
                trendSheet.Cell(row, 2).Value = week.WeekIndex;
                trendSheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
                trendSheet.Cell(row, 3).Value = week.WeekStartDate;
                trendSheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                trendSheet.Cell(row, 4).Value = week.WeekEndDate;
                trendSheet.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                trendSheet.Cell(row, 5).Value = week.ActivityCount;
                trendSheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                trendSheet.Cell(row, 6).Value = week.TotalScoreInWeek;
                trendSheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                trendSheet.Cell(row, 7).Value = week.ParticipatedStudentCount;
                trendSheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                row++;
            }
            
            // Enable AutoFilter và format borders
            var dataRange = trendSheet.Range(1, 1, row - 1, 7);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Black;
            dataRange.Style.Border.InsideBorderColor = XLColor.Gray;
            
            // Zebra striping
            for (int i = 2; i < row; i++)
            {
                if (i % 2 == 0)
                {
                    trendSheet.Range(i, 1, i, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                }
            }
            
            trendSheet.Range(1, 1, row - 1, 7).SetAutoFilter();
            
            trendSheet.Columns().AdjustToContents();
        }
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private string GenerateHtmlReport(AcademicYearStatisticsReport report, StatisticsReportRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='UTF-8'>");
        sb.AppendLine("<title>Báo cáo thống kê năm học " + report.AcademicYear + "</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { color: #1F2937; }");
        sb.AppendLine("h2 { color: #374151; margin-top: 30px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        sb.AppendLine("th, td { border: 1px solid #E5E7EB; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #F3F4F6; font-weight: bold; }");
        sb.AppendLine(".metric-card { display: inline-block; margin: 10px; padding: 15px; border: 1px solid #E5E7EB; border-radius: 4px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        sb.AppendLine("<h1>Báo cáo thống kê năm học " + report.AcademicYear + "</h1>");
        
        if (report.Overview != null)
        {
            sb.AppendLine("<h2>Tổng quan</h2>");
            sb.AppendLine("<div class='metric-card'>");
            sb.AppendLine("<strong>Tổng số lớp:</strong> " + report.Overview.TotalClasses);
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='metric-card'>");
            sb.AppendLine("<strong>Tổng số học sinh:</strong> " + report.Overview.TotalStudents);
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='metric-card'>");
            sb.AppendLine("<strong>Tổng điểm toàn trường:</strong> " + report.Overview.TotalSchoolScore.ToString("N0"));
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='metric-card'>");
            sb.AppendLine("<strong>Số hoạt động:</strong> " + report.Overview.TotalActivitiesInYear);
            sb.AppendLine("</div>");
        }
        
        if (request.IncludeClassRewardTable && report.Classes != null && report.Classes.Count > 0)
        {
            sb.AppendLine("<h2>Bảng điểm theo lớp</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Hạng</th>");
            sb.AppendLine("<th>Lớp</th>");
            sb.AppendLine("<th>GVCN</th>");
            sb.AppendLine("<th>Số HS</th>");
            sb.AppendLine("<th>Tổng điểm</th>");
            sb.AppendLine("<th>TB/HS</th>");
            sb.AppendLine("<th>% tổng điểm</th>");
            sb.AppendLine("<th>Số hoạt động</th>");
            sb.AppendLine("</tr>");
            
            foreach (var cls in report.Classes)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + cls.RankByScore + "</td>");
                sb.AppendLine("<td>" + cls.ClassCode + "</td>");
                sb.AppendLine("<td>" + cls.HomeroomTeacherName + "</td>");
                sb.AppendLine("<td>" + cls.StudentCount + "</td>");
                sb.AppendLine("<td>" + cls.TotalScoreInYear.ToString("N0") + "</td>");
                sb.AppendLine("<td>" + cls.AverageScorePerStudent.ToString("F1") + "</td>");
                sb.AppendLine("<td>" + cls.ScoreShareOfSchoolTotal.ToString("F1") + "%</td>");
                sb.AppendLine("<td>" + cls.ActivityCountInYear + "</td>");
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("</table>");
        }
        
        if (request.IncludeStudentRewardTable && report.Students != null && report.Students.Count > 0)
        {
            sb.AppendLine("<h2>Bảng điểm theo học sinh</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Mã HS</th>");
            sb.AppendLine("<th>Họ tên</th>");
            sb.AppendLine("<th>Lớp</th>");
            sb.AppendLine("<th>Tổng điểm</th>");
            sb.AppendLine("<th>Số hoạt động</th>");
            sb.AppendLine("</tr>");
            
            foreach (var student in report.Students.Take(1000))
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + student.StudentCode + "</td>");
                sb.AppendLine("<td>" + student.StudentFullName + "</td>");
                sb.AppendLine("<td>" + student.ClassCode + "</td>");
                sb.AppendLine("<td>" + student.TotalScoreInYear.ToString("N0") + "</td>");
                sb.AppendLine("<td>" + student.ActivityCountInYear + "</td>");
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("</table>");
        }
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }

}

