using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.UnitTests.SharedResults;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_wraps_data_and_marks_success()
    {
        var response = ApiResponse<string>.Ok("hello", message: "done", correlationId: "abc");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("hello");
        response.Message.Should().Be("done");
        response.CorrelationId.Should().Be("abc");
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_carries_error_and_marks_failure()
    {
        var error = Error.Validation("field.invalid", "Field is required.");
        var response = ApiResponse<string>.Fail(error, correlationId: "xyz");

        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be("field.invalid");
        response.Error.Message.Should().Be("Field is required.");
        response.Error.Type.Should().Be("Validation");
        response.CorrelationId.Should().Be("xyz");
        response.Data.Should().BeNull();
    }
}
