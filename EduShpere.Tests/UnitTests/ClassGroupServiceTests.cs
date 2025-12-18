using Xunit;
using Moq;
using FluentAssertions;
using EduShpere.Application.Services.ClassGroupService;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Infrastructure;
using EduShpere.Application.Services;
using AutoMapper;

namespace EduShpere.Tests.UnitTests;

public class ClassGroupServiceTests
{
    private readonly Mock<IClassGroupRepository> _mockRepository;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<IPaginationService> _mockPaginationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ClassGroupService _service;

    public ClassGroupServiceTests()
    {
        _mockRepository = new Mock<IClassGroupRepository>();
        _mockAuditService = new Mock<IAuditService>();
        _mockPaginationService = new Mock<IPaginationService>();
        _mockMapper = new Mock<IMapper>();
        
        _service = new ClassGroupService(
            _mockRepository.Object,
            _mockAuditService.Object,
            _mockPaginationService.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsClassGroupDto()
    {
        // Arrange
        var classGroupId = 1;
        var classGroup = new EduShpere.Domain.Models.ClassGroup
        {
            Id = classGroupId,
            Name = "10A1",
            IsDeleted = false
        };

        var expectedDto = new ClassGroupDto
        {
            Id = classGroupId,
            Name = "10A1"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(classGroupId))
            .ReturnsAsync(classGroup);
        _mockMapper.Setup(m => m.Map<ClassGroupDto>(classGroup))
            .Returns(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(classGroupId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(classGroupId);
        result.Name.Should().Be("10A1");
        _mockRepository.Verify(r => r.GetByIdAsync(classGroupId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var classGroupId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(classGroupId))
            .ReturnsAsync((EduShpere.Domain.Models.ClassGroup?)null);

        // Act
        var result = await _service.GetByIdAsync(classGroupId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(classGroupId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedClassGroup_ReturnsNull()
    {
        // Arrange
        var classGroupId = 1;
        var deletedClassGroup = new EduShpere.Domain.Models.ClassGroup
        {
            Id = classGroupId,
            Name = "10A1",
            IsDeleted = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(classGroupId))
            .ReturnsAsync(deletedClassGroup);

        // Act
        var result = await _service.GetByIdAsync(classGroupId);

        // Assert
        result.Should().BeNull();
    }
}
