using Application.Common.Models;
using Xunit;

namespace UnitTests.Application;

public class ResultTests
{
    [Fact]
    public void Result_Success_ShouldHaveIsSuccessTrueAndStatusCode200()
    {
        var result = Result.Success("Done");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Done", result.Message);
    }

    [Fact]
    public void Result_Failure_ShouldHaveIsFailureTrueAndErrors()
    {
        var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error message" } } };
        var result = Result.Failure("Failed", 400, errors);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Failed", result.Message);
        Assert.NotNull(result.Errors);
        Assert.Contains("Field", result.Errors.Keys);
    }

    [Fact]
    public void GenericResult_Success_ShouldHoldDataAndValue()
    {
        var result = Result<string>.Success("Payload data");

        Assert.True(result.IsSuccess);
        Assert.Equal("Payload data", result.Data);
        Assert.Equal("Payload data", result.Value);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void GenericResult_ImplicitConversion_ShouldCreateSuccessResult()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void GenericResult_BadRequest_ShouldHave400StatusCode()
    {
        var result = Result<string>.BadRequest("Invalid parameters");

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid parameters", result.Message);
    }

    [Fact]
    public void GenericResult_Unauthorized_ShouldHave401StatusCode()
    {
        var result = Result<string>.Unauthorized();

        Assert.True(result.IsFailure);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("Unauthorized access.", result.Message);
    }

    [Fact]
    public void GenericResult_Forbidden_ShouldHave403StatusCode()
    {
        var result = Result<string>.Forbidden();

        Assert.True(result.IsFailure);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Access to this resource is forbidden.", result.Message);
    }

    [Fact]
    public void GenericResult_NotFound_ShouldHave404StatusCode()
    {
        var result = Result<string>.NotFound("Project not found.");

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Project not found.", result.Message);
    }

    [Fact]
    public void GenericResult_Conflict_ShouldHave409StatusCode()
    {
        var result = Result<string>.Conflict("Duplicate email address.");

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Duplicate email address.", result.Message);
    }

    [Fact]
    public void GenericResult_InternalServerError_ShouldHave500StatusCode()
    {
        var result = Result<string>.InternalServerError("Database connection dropped.");

        Assert.True(result.IsFailure);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Database connection dropped.", result.Message);
    }

    [Fact]
    public void ApiResponse_ShouldInheritFromResult()
    {
        var apiResponse = ApiResponse<string>.Success("Inherited");

        Assert.True(apiResponse is Result<string>);
        Assert.True(apiResponse.IsSuccess);
        Assert.Equal("Inherited", apiResponse.Data);
    }

    [Fact]
    public void ApiResponse_Serialization_ShouldNotContainDuplicateValueProperty()
    {
        var apiResponse = ApiResponse<string>.Success("Sample Payload");
        var json = System.Text.Json.JsonSerializer.Serialize(apiResponse, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("data", out var dataProp));
        Assert.Equal("Sample Payload", dataProp.GetString());

        Assert.False(root.TryGetProperty("value", out _));
    }
}
