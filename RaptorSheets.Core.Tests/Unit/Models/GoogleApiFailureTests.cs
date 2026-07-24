using System.Net;
using RaptorSheets.Core.Models;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Models;

public class GoogleApiFailureTests
{
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, GoogleApiFailureReason.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, GoogleApiFailureReason.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, GoogleApiFailureReason.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests, GoogleApiFailureReason.QuotaExceeded)]
    [InlineData(HttpStatusCode.InternalServerError, GoogleApiFailureReason.Unknown)]
    public void FromException_ShouldMapGoogleApiExceptionStatusCodeToReason(HttpStatusCode statusCode, GoogleApiFailureReason expectedReason)
    {
        var exception = new Google.GoogleApiException("sheets") { HttpStatusCode = statusCode };

        var failure = InvokeFromException(exception);

        Assert.Equal(expectedReason, failure.Reason);
        Assert.Equal(statusCode, failure.StatusCode);
    }

    [Fact]
    public void FromException_WithNonApiException_ShouldClassifyAsUnknownWithNoStatusCode()
    {
        // Transport-level failures (DNS, socket, timeout) never reach the API - there is no HTTP
        // status to classify, so this must not throw trying to find one.
        var exception = new HttpRequestException("network unreachable");

        var failure = InvokeFromException(exception);

        Assert.Equal(GoogleApiFailureReason.Unknown, failure.Reason);
        Assert.Null(failure.StatusCode);
        Assert.Equal(exception.Message, failure.Message);
    }

    [Fact]
    public void QuotaExceeded_MustNeverBeMistakenForNotFound()
    {
        // The whole point of naming this reason: a caller branching on it must be able to tell
        // "try again" apart from "the spreadsheet is gone" without inspecting a status code.
        var exception = new Google.GoogleApiException("sheets") { HttpStatusCode = HttpStatusCode.TooManyRequests };

        var failure = InvokeFromException(exception);

        Assert.Equal(GoogleApiFailureReason.QuotaExceeded, failure.Reason);
        Assert.NotEqual(GoogleApiFailureReason.NotFound, failure.Reason);
    }

    [Fact]
    public void Result_Ok_ShouldCarryTheValueAndNoFailure()
    {
        var result = GoogleApiResult<string>.Ok("payload");

        Assert.True(result.Success);
        Assert.Equal("payload", result.Value);
        Assert.Null(result.Failure);
    }

    [Fact]
    public void Result_Failed_ShouldCarryTheFailureAndDefaultValue()
    {
        var failure = new GoogleApiFailure { Reason = GoogleApiFailureReason.QuotaExceeded, Message = "rate limited" };

        var result = GoogleApiResult<string>.Failed(failure);

        Assert.False(result.Success);
        Assert.Null(result.Value);
        Assert.Same(failure, result.Failure);
    }

    private static GoogleApiFailure InvokeFromException(Exception ex)
    {
        // FromException is internal by design - callers get it attached to a result, not the
        // classification logic itself - so tests reach it via InternalsVisibleTo.
        return GoogleApiFailure.FromException(ex);
    }
}
