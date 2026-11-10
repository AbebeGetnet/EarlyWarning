using System.ComponentModel.DataAnnotations;
using EarlyWarning.Models;

namespace EarlyWarning.Tests;

/// <summary>GAM and MAM rates are calculated from the screening counts; found cannot exceed screened.</summary>
public class MalnutritionReportTests
{
    [Fact]
    public void CalculateRates_UsesSevereAndModerateForGam_AndModerateForMam()
    {
        var report = new MalnutritionReport
        {
            ChildrenScreened = 300, ChildrenSevere = 4, ChildrenModerate = 20,
            PregnantLactatingScreened = 120, PregnantLactatingModerate = 9
        };

        report.CalculateRates();

        Assert.Equal(8.00m, report.ChildrenGamRatePercent);           // 24 / 300
        Assert.Equal(7.50m, report.PregnantLactatingMamRatePercent);  // 9 / 120
    }

    [Fact]
    public void CalculateRates_NobodyScreened_GivesNoRate()
    {
        var report = new MalnutritionReport { ChildrenGamRatePercent = 50, PregnantLactatingMamRatePercent = 50 };

        report.CalculateRates();

        Assert.Null(report.ChildrenGamRatePercent);
        Assert.Null(report.PregnantLactatingMamRatePercent);
    }

    [Fact]
    public void Rate_RoundsToTwoDecimals() => Assert.Equal(33.33m, MalnutritionReport.Rate(1, 3));

    [Fact]
    public void Validate_RejectsMoreFoundThanScreened()
    {
        var report = new MalnutritionReport
        {
            ChildrenScreened = 10, ChildrenSevere = 6, ChildrenModerate = 5,
            PregnantLactatingScreened = 3, PregnantLactatingModerate = 4
        };

        var errors = report.Validate(new ValidationContext(report)).ToList();

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(MalnutritionReport.ChildrenSevere)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(MalnutritionReport.PregnantLactatingModerate)));
    }

    [Fact]
    public void Validate_AcceptsEmptyReport() =>
        Assert.Empty(new MalnutritionReport().Validate(new ValidationContext(new MalnutritionReport())));
}
