using System.Collections.Concurrent;
using System.Reflection;

namespace EarlyWarning.Extensions;

/// <summary>
/// One beneficiary table (አባወራ/እማወራ, ቤተሰብ, ህፃናት, ወጣቶች, አረጋውያን, አካል ጉዳተኞች).
/// <para>
/// Primary rows — household heads and family members — are the people counted. The other rows are
/// distributions of those same people: by age (children under 5, youth, elderly — mutually exclusive
/// but not exhaustive, as there is no row for other adults) and by health status (disabled). They are
/// never added to the total.
/// </para>
/// Male and female are counted and checked separately. This is the single place the rule lives;
/// <c>wwwroot/js/beneficiaries.js</c> mirrors it for the forms.
/// </summary>
public readonly record struct Beneficiaries(
    long MaleHouseholds, long FemaleHouseholds, long MaleFamily, long FemaleFamily,
    long MaleChildren, long FemaleChildren, long MaleYouth, long FemaleYouth,
    long MaleElderly, long FemaleElderly, long MaleDisabled, long FemaleDisabled)
{
    public long Male => MaleHouseholds + MaleFamily;
    public long Female => FemaleHouseholds + FemaleFamily;
    public long Total => Male + Female;

    public long MaleByAge => MaleChildren + MaleYouth + MaleElderly;
    public long FemaleByAge => FemaleChildren + FemaleYouth + FemaleElderly;

    /// <summary>The beneficiary count from the primary rows only.</summary>
    public static long Count(long maleHouseholds, long femaleHouseholds, long maleFamily, long femaleFamily) =>
        maleHouseholds + femaleHouseholds + maleFamily + femaleFamily;

    public static Beneficiaries operator +(Beneficiaries a, Beneficiaries b) => new(
        a.MaleHouseholds + b.MaleHouseholds, a.FemaleHouseholds + b.FemaleHouseholds,
        a.MaleFamily + b.MaleFamily, a.FemaleFamily + b.FemaleFamily,
        a.MaleChildren + b.MaleChildren, a.FemaleChildren + b.FemaleChildren,
        a.MaleYouth + b.MaleYouth, a.FemaleYouth + b.FemaleYouth,
        a.MaleElderly + b.MaleElderly, a.FemaleElderly + b.FemaleElderly,
        a.MaleDisabled + b.MaleDisabled, a.FemaleDisabled + b.FemaleDisabled);

    /// <summary>
    /// Distribution checks, per sex: the age groups together, and the disabled, may not exceed the
    /// primary total. Equality is not required because the age groups do not cover every adult.
    /// </summary>
    public IEnumerable<string> Problems()
    {
        foreach (var (sex, primary, byAge, disabled) in new[] { ("ወንድ", Male, MaleByAge, MaleDisabled), ("ሴት", Female, FemaleByAge, FemaleDisabled) })
        {
            if (byAge > primary)
                yield return $"{sex}፡ ህፃናት + ወጣቶች + አረጋውያን ({byAge:#,0}) ከአባወራ/እማወራ + ቤተሰብ ድምር ({primary:#,0}) መብለጥ አይችልም።";
            if (disabled > primary)
                yield return $"{sex}፡ አካል ጉዳተኞች ({disabled:#,0}) ከአባወራ/እማወራ + ቤተሰብ ድምር ({primary:#,0}) መብለጥ አይችልም።";
        }
    }

    // The three property-name schemes used by the beneficiary tables, in the row order of the constructor.
    // A missing name (accidents have no family row) reads as 0.
    private static readonly string?[][] Schemes =
    [
        ["MaleHouseholdHeads", "FemaleHouseholdHeads", "MaleFamilyMembers", "FemaleFamilyMembers", "MaleChildren", "FemaleChildren",
         "MaleYouth", "FemaleYouth", "MaleElderly", "FemaleElderly", "MaleDisabled", "FemaleDisabled"],
        ["MaleHouseHold", "FemaleHouseHold", "MaleFamily", "FemaleFamily", "ChildhMale", "ChildFemale",
         "YouthMale", "YouthFemale", "ElderlyMale", "ElderlyFemale", "DisabledMale", "DisabledFemale"],
        ["AffectedHouseholdsMale", "AffectedHouseholdsFemale", null, null, "AffectedChildrenMale", "AffectedChildrenFemale",
         "AffectedYouthMale", "AffectedYouthFemale", "AffectedElderlyMale", "AffectedElderlyFemale", "AffectedDisabledMale", "AffectedDisabledFemale"]
    ];

    private static readonly ConcurrentDictionary<Type, PropertyInfo?[]?> Accessors = new();

    /// <summary>Reads any record holding a beneficiary table; null when the type has none.</summary>
    public static Beneficiaries? From(object? source)
    {
        if (source == null) return null;
        var props = Accessors.GetOrAdd(source.GetType(), type =>
        {
            foreach (var scheme in Schemes)
            {
                if (type.GetProperty(scheme[0]!) == null) continue;
                return scheme.Select(name => name == null ? null : type.GetProperty(name)).ToArray();
            }
            return null;
        });
        if (props == null) return null;
        long V(int i) => props[i]?.GetValue(source) is { } value ? Convert.ToInt64(value) : 0;
        return new Beneficiaries(V(0), V(1), V(2), V(3), V(4), V(5), V(6), V(7), V(8), V(9), V(10), V(11));
    }
}
