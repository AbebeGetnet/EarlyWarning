using System.Text.RegularExpressions;
using EarlyWarning.Enums;
using EarlyWarning.Extensions;
using EarlyWarning.Models;
using EarlyWarning.Models.Migration;
using EarlyWarning.Models.WeeklyAccidents;
using EarlyWarning.ViewModels;

namespace EarlyWarning.Tests;

/// <summary>
/// The approval details page shows every submitted record read-only. Every label and status must be in Amharic,
/// and nothing may be shown twice.
/// </summary>
public class DetailsPageLabelsTests
{
    private static readonly Regex English = new("[A-Za-z]");

    public static TheoryData<Type> FormRecordTypes()
    {
        var data = new TheoryData<Type>();
        foreach (var type in typeof(WeeklySubmissionFormViewModel).GetProperties()
                     .Select(p => Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)
                     .Where(t => t.IsClass && t != typeof(string) && !t.IsGenericType))
            data.Add(type);
        data.Add(typeof(AccidentDetail));
        data.Add(typeof(MigrationDetail));
        data.Add(typeof(DeathDetail));
        return data;
    }

    [Theory]
    [MemberData(nameof(FormRecordTypes))]
    public void EveryFieldLabelIsAmharicAndShownOnce(Type recordType)
    {
        var labels = ReadOnlyFields.For(Activator.CreateInstance(recordType)).Select(x => x.Label).ToList();

        Assert.DoesNotContain(labels, label => English.IsMatch(label));
        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void WorkflowColumnsAndRepeatedValuesAreNotShownAsSubmittedData()
    {
        Assert.DoesNotContain(ReadOnlyFields.For(new Models.HumanDrinkWaterIssue.HumanDrinkWaterIssue()), x => x.Label.Contains("በሂደት"));
        Assert.DoesNotContain(ReadOnlyFields.For(new Models.Aid.WeeklyProvided()), x => x.Label == "የሪፖርት ሁኔታ");
        Assert.DoesNotContain(ReadOnlyFields.For(new AccidentDetail()), x => x.Label == "የጉዳት ምድቦች"); // shown as checkboxes
        Assert.DoesNotContain(ReadOnlyFields.For(new RainfallReport()), x => x.Label.EndsWith("List"));
    }

    [Fact]
    public void PerTypeTotalsAndPeopleAreSkippedWhenTheTableShowsThem()
    {
        var crop = ReadOnlyFields.For(new CropPestAndDeseaseReport(), [.. ReadOnlyFields.ReportLevelPeople, "AffectedLandInHectar"]);
        var health = ReadOnlyFields.For(new AnimalHealthStatus(), [.. ReadOnlyFields.ReportLevelPeople, "NoOfKebeliesWithWaterSupply"]);

        Assert.DoesNotContain(crop, x => x.Label.Contains("ጠቅላላ ድምር") || x.Label.Contains("አባወራ"));
        Assert.DoesNotContain(health, x => x.Label.Contains("ጠቅላላ ድምር") || x.Label.Contains("አባወራ"));
        Assert.Contains(crop, x => x.Label == "የሰብል ተባይ እና በሽታ አጋጥሟል?");
    }

    [Fact]
    public void WorkflowStatusesAndActionsHaveAmharicLabels()
    {
        foreach (var status in Enum.GetValues<WeeklySubmissionStatus>())
            Assert.DoesNotMatch(English, ReadOnlyFields.EnumLabel(status));
        foreach (var action in Enum.GetValues<WeeklyWorkflowAction>())
            Assert.DoesNotMatch(English, ReadOnlyFields.EnumLabel(action));
        foreach (var animal in Enum.GetValues<MigratingAnimalType>())
            Assert.DoesNotMatch(English, ReadOnlyFields.EnumLabel(animal));
    }
}
