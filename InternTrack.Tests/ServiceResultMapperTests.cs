using System.Text.Json;
using InternTrack.Api.Helpers;
using InternTrack.Business.Common;
using Microsoft.AspNetCore.Mvc;

namespace InternTrack.Tests;

public class ServiceResultMapperTests
{
    [Theory]
    [InlineData(ResultType.ValidationError, 400)]
    [InlineData(ResultType.NotFound, 404)]
    [InlineData(ResultType.Conflict, 409)]
    [InlineData(ResultType.Forbidden, 403)]
    [InlineData((ResultType)999, 400)]
    public void FailureMapping_ShouldPreserveStatusAndMessageForBothResultTypes(ResultType type, int statusCode)
    {
        var controller = new TestController();
        const string message = "Existing error message";
        var result = new ServiceResult { Type = type, Message = message };
        var genericResult = new ServiceResult<string> { Type = type, Message = message };

        var mappedResults = new[]
        {
            ServiceResultMapper.ToActionResult(controller, result),
            ServiceResultMapper.ToActionResult(controller, genericResult)
        };

        foreach (var mapped in mappedResults)
        {
            var response = Assert.IsAssignableFrom<ObjectResult>(mapped);
            Assert.Equal(statusCode, response.StatusCode);
            var body = JsonSerializer.SerializeToElement(response.Value);
            Assert.Equal(message, body.GetProperty("message").GetString());
            Assert.Single(body.EnumerateObject());
        }
    }

    [Fact]
    public void SuccessMapping_ShouldPreserveMessageDataAndNoContentContracts()
    {
        var controller = new TestController();
        var result = ServiceResult.Ok("Created");
        var data = new { Id = 42 };

        var created = Assert.IsType<ObjectResult>(ServiceResultMapper.ToActionResult(controller, result, 201));
        Assert.Equal(201, created.StatusCode);
        Assert.Equal("Created", JsonSerializer.SerializeToElement(created.Value).GetProperty("message").GetString());
        Assert.IsType<NoContentResult>(ServiceResultMapper.ToActionResult(controller, result, noContentOnSuccess: true));
        var retrieved = Assert.IsType<OkObjectResult>(ServiceResultMapper.ToActionResult(controller, ServiceResult<object>.Ok(data)));
        Assert.Same(data, retrieved.Value);
    }

    private sealed class TestController : ControllerBase
    {
    }
}
