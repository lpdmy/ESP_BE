using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using EduShpere;

namespace EduShpere.Tests.IntegrationTests;

public class ClassGroupControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClassGroupControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/classgroup?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var classGroupId = 1;

        // Act
        var response = await _client.GetAsync($"/api/classgroup/{classGroupId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHomeroomTeacher_WithValidId_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var classGroupId = 1;

        // Act
        var response = await _client.GetAsync($"/api/classgroup/{classGroupId}/homeroom-teacher");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHomeroomTeachers_Batch_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var classGroupIds = new List<int> { 1, 2, 3 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/classgroup/homeroom-teachers", classGroupIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
