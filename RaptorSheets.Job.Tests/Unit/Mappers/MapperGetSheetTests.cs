using RaptorSheets.Job.Mappers;

namespace RaptorSheets.Job.Tests.Unit.Mappers;

/// <summary>
/// Essential tests to verify all mappers can successfully call GetSheet() without errors.
/// These tests ensure there are no circular dependencies or initialization issues.
/// </summary>
public class MapperGetSheetTests
{
    [Fact]
    public void AllMappers_GetSheet_ShouldSucceed()
    {
        // Act & Assert - Verify each mapper can generate its sheet without throwing
        var applicationSheet = Record.Exception(() => ApplicationMapper.GetSheet());
        Assert.Null(applicationSheet);

        var interviewSheet = Record.Exception(() => InterviewMapper.GetSheet());
        Assert.Null(interviewSheet);

        var companySheet = Record.Exception(() => CompanyMapper.GetSheet());
        Assert.Null(companySheet);

        var positionSheet = Record.Exception(() => PositionMapper.GetSheet());
        Assert.Null(positionSheet);

        var siteSheet = Record.Exception(() => ValidationMapper.GetSiteSheet());
        Assert.Null(siteSheet);

        var decisionSheet = Record.Exception(() => ValidationMapper.GetDecisionSheet());
        Assert.Null(decisionSheet);

        var interviewTypeSheet = Record.Exception(() => ValidationMapper.GetInterviewTypeSheet());
        Assert.Null(interviewTypeSheet);

        var interviewOutcomeSheet = Record.Exception(() => ValidationMapper.GetInterviewOutcomeSheet());
        Assert.Null(interviewOutcomeSheet);

        var scheduleSheet = Record.Exception(() => ValidationMapper.GetScheduleSheet());
        Assert.Null(scheduleSheet);

        var setupSheet = Record.Exception(() => ValidationMapper.GetSetupSheet());
        Assert.Null(setupSheet);
    }

    [Fact]
    public void ValidationMappers_CallingApplicationMapper_ShouldNotCauseCircularDependency()
    {
        // Arrange & Act - ValidationMapper calls ApplicationMapper during configuration
        var exception = Record.Exception(() =>
        {
            var siteSheet = ValidationMapper.GetSiteSheet();
            var decisionSheet = ValidationMapper.GetDecisionSheet();
            var scheduleSheet = ValidationMapper.GetScheduleSheet();
        });

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void ApplicationMapper_CallingInterviewMapper_ShouldNotCauseCircularDependency()
    {
        // Arrange & Act - ApplicationMapper calls InterviewMapper during configuration
        var exception = Record.Exception(() =>
        {
            var appSheet = ApplicationMapper.GetSheet();
        });

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void CompanyMapper_CallingOtherMappers_ShouldNotCauseCircularDependency()
    {
        // Arrange & Act - CompanyMapper calls ApplicationMapper and InterviewMapper
        var exception = Record.Exception(() =>
        {
            var companySheet = CompanyMapper.GetSheet();
        });

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void PositionMapper_CallingApplicationMapper_ShouldNotCauseCircularDependency()
    {
        // Arrange & Act - PositionMapper calls ApplicationMapper
        var exception = Record.Exception(() =>
        {
            var positionSheet = PositionMapper.GetSheet();
        });

        // Assert - Should not throw
        Assert.Null(exception);
    }

    [Fact]
    public void MultipleMapperCalls_ShouldBeIdempotent()
    {
        // Act - Call same mapper multiple times
        var sheet1 = ApplicationMapper.GetSheet();
        var sheet2 = ApplicationMapper.GetSheet();
        var sheet3 = ApplicationMapper.GetSheet();

        // Assert - Should return valid sheets each time
        Assert.NotNull(sheet1);
        Assert.NotNull(sheet2);
        Assert.NotNull(sheet3);

        Assert.Equal(sheet1.Name, sheet2.Name);
        Assert.Equal(sheet1.Headers.Count, sheet2.Headers.Count);
    }
}
