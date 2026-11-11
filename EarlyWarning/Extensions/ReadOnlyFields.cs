using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;

namespace EarlyWarning.Extensions;

/// <summary>
/// Turns a submitted domain record into label/value pairs for the read-only approval form, skipping keys,
/// audit/workflow columns, JSON blobs, navigation properties and computed totals.
/// </summary>
public static class ReadOnlyFields
{
    private static readonly HashSet<string> RetiredWhenEmpty = ["MajorSolutionTwo", "MajorSolutionThree"];

    private static readonly HashSet<string> Excluded =
    [
        "UserId", "LasModifiedAt", "DeletedAt", "StartDate", "EndDate", "Status", "ReportDate", "ReportedBy", "ApprovalStatus",
        // Workflow columns: approval is shown by the workflow, not as submitted data.
        "ZonalApproval", "ReportStatus",
        // Shown elsewhere on the form: the damage categories as checkboxes; crop stages in their own section.
        "DamageCategories", "CropGrowth"
    ];

    // Properties without an Amharic display name on the model.
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["MeherFarmPlan"] = "መኸር - የታቀደ (ሄ/ር)",
        ["MeherPloughed"] = "መኸር - የታረሰ (ሄ/ር)",
        ["MeherSown"] = "መኸር - የተዘራ (ሄ/ር)",
        ["MeherHarvestingHHarvesting"] = "መኸር - የተሰበሰበ (ሄ/ር)",
        ["MeherSownWithResidualMoisture"] = "መኸር - በቀሪ እርጥበት የተዘራ (ሄ/ር)",
        ["AutumnFarmPlan"] = "በልግ - የታቀደ (ሄ/ር)",
        ["AutumnPloughed"] = "በልግ - የታረሰ (ሄ/ር)",
        ["AutumnSown"] = "በልግ - የተዘራ (ሄ/ር)",
        ["AutumnHarvestingHHarvesting"] = "በልግ - የተሰበሰበ (ሄ/ር)",
        ["AutumnSownWithResidualMoisture"] = "በልግ - በቀሪ እርጥበት የተዘራ (ሄ/ር)",
        ["AtSeedStage"] = "በዘር ደረጃ (ሄ/ር)",
        ["AtGerminationStage"] = "በብቅለት ደረጃ (ሄ/ር)",
        ["AtGrowthStage"] = "በእድገት ደረጃ (ሄ/ር)",
        ["AtFruitStage"] = "ፍሬ በመሙላት ደረጃ (ሄ/ር)",
        ["AtHarvestingStage"] = "በመሰብሰብ ደረጃ (ሄ/ር)",
        ["Cause"] = "የችግሩ ምክንያት",
        ["MaleStudentDropouts"] = "ከትምህርት የወጡ ወንድ ተማሪዎች",
        ["FemaleStudentDropouts"] = "ከትምህርት የወጡ ሴት ተማሪዎች",
        ["AverageDropoutAge"] = "የወጡ ተማሪዎች አማካይ ዕድሜ",
        ["ClosedSchools"] = "የተዘጉ ትምህርት ቤቶች ብዛት",
        ["MajorSolutionOne"] = "ችግሩን ለመቆጣጠር/ለመፍታት የተወሰደ ዋና መፍትሄ",
        ["MajorSolutionTwo"] = "መፍትሄ 2",
        ["MajorSolutionThree"] = "መፍትሄ 3",
        ["WasActionSatisfactory"] = "የተወሰደው እርምጃ አጥጋቢ ነበር?",
        ["StudentRelatedSolution"] = "መፍትሄ፡ የተማሪ ጉዳዮች",
        ["TeacherExpenseSolution"] = "መፍትሄ፡ ከመምህራን ጋር የተያያዙ ወጪዎች",
        ["ClassroomMaterialsSolution"] = "መፍትሄ፡ ክፍል ጥገናና የመማሪያ ቁሳቁስ",
        ["SchoolBuildingServiceSolution"] = "መፍትሄ፡ የትምህርት ቤት ሕንፃ አገልግሎት",
        ["OtherSolution"] = "መፍትሄ፡ ሌላ",
        ["OtherSolutionDescription"] = "ሌላ መፍትሄ"
    };

    /// <summary>Report-level affected-people columns, which repeat the per-type tables when those exist.</summary>
    public static readonly string[] ReportLevelPeople =
    [
        "MaleHouseHold", "FemaleHouseHold", "MaleFamily", "FemaleFamily", "ChildhMale", "ChildFemale",
        "YouthMale", "YouthFemale", "ElderlyMale", "ElderlyFemale", "DisabledMale", "DisabledFemale"
    ];

    // Many demographic columns are only labelled "ወንድ"/"ሴት"; the property name says which group they belong to.
    private static readonly (string NamePart, string Group)[] Groups =
    [
        ("HouseHold", "አባወራ/እማወራ"), ("Household", "አባወራ/እማወራ"), ("Family", "ቤተሰብ"), ("Child", "ህፃናት ከ5 ዓመት በታች"),
        ("Youth", "ወጣቶች"), ("Elderly", "አረጋውያን"), ("Disabled", "አካል ጉዳተኞች")
    ];

    /// <param name="skip">Properties already shown elsewhere on the form (e.g. totals of a per-type table).</param>
    public static IReadOnlyList<(string Label, string Value)> For(object? source, params string[] skip)
    {
        if (source == null) return [];
        return source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Visible(p) && !skip.Contains(p.Name))
            .Select(p => (Property: p, Value: Format(p.GetValue(source))))
            // Fields no longer collected appear only on older records that filled them.
            .Where(x => !RetiredWhenEmpty.Contains(x.Property.Name) || !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => (Label(x.Property), x.Value))
            .ToList();
    }

    public static string Format(object? value) => value switch
    {
        null => "",
        bool b => b ? "አዎ" : "የለም",
        DateTime d => d.ToString("yyyy-MM-dd"),
        decimal or double or float => Convert.ToDecimal(value).ToString("#,0.##", CultureInfo.InvariantCulture),
        Enum e => EnumLabel(e),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
    };

    public static string EnumLabel(Enum value) =>
        value.GetType().GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString().Replace('_', ' ');

    private static bool Visible(PropertyInfo p)
    {
        if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0) return false;
        if (p.GetCustomAttribute<NotMappedAttribute>() != null) return false;
        if (Excluded.Contains(p.Name) || p.Name == "Id" || p.Name.EndsWith("Id") || p.Name.EndsWith("Json")) return false;
        // Old free-text kebele lists; the form shows the selected kebeles by name instead.
        if (p.Name.EndsWith("List")) return false;
        if (p.Name.StartsWith("Zone") || p.Name.StartsWith("Region") || p.Name.StartsWith("Submitted")) return false;
        var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }

    private static string Label(PropertyInfo p)
    {
        var label = p.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
            ?? Labels.GetValueOrDefault(p.Name) ?? p.Name;
        label = label.Trim();
        // Same "group (ወንድ/ሴት)" form as the other people fields.
        if (label is "ወንድ" or "ሴት")
        {
            var group = Groups.FirstOrDefault(g => p.Name.Contains(g.NamePart)).Group;
            if (group != null) label = $"{group} ({label})";
        }
        return label;
    }
}
