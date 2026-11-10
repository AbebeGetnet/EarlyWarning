using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

/// <summary>
/// Weekly malnutrition (የስነ-ምግብ እጥረት) screening results for one woreda:
/// children under five and pregnant/lactating women.
/// The rates are calculated from the counts (see <see cref="CalculateRates"/>), never typed in.
/// </summary>
public class MalnutritionReport : IValidatableObject
{
    public Guid MalnutritionReportId { get; set; } = Guid.NewGuid();
    public Guid WoredaId { get; set; }
    [ForeignKey(nameof(WoredaId))] public Locations? Woreda { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // ---- 1. Children under five (ከ5 ዓመት በታች ያሉ ህጻናት)
    [Display(Name = "የስነ-ምግብ ልየታ የተደረገላቸው ከ5 ዓመት በታች ህጻናት ብዛት")]
    [Range(0, 10_000_000)] public int ChildrenScreened { get; set; }

    [Display(Name = "በከፍተኛ ደረጃ የስነ-ምግብ እጥረት የተገኘባቸው ህጻናት ብዛት")]
    [Range(0, 10_000_000)] public int ChildrenSevere { get; set; }

    [Display(Name = "በመካከለኛ ደረጃ የስነ-ምግብ እጥረት የተገኘባቸው ህጻናት ብዛት")]
    [Range(0, 10_000_000)] public int ChildrenModerate { get; set; }

    [Display(Name = "GAM Rate (%)")]
    [Column(TypeName = "decimal(9,2)")]
    [Range(typeof(decimal), "0", "100")] public decimal? ChildrenGamRatePercent { get; set; }

    // ---- 2. Pregnant and lactating women (እርጉዝና የሚያጠቡ እናቶች)
    [Display(Name = "የስነ-ምግብ ልየታ የተደረገላቸው እርጉዝና የሚያጠቡ እናቶች ብዛት")]
    [Range(0, 10_000_000)] public int PregnantLactatingScreened { get; set; }

    [Display(Name = "በመካከለኛ ደረጃ የስነ-ምግብ እጥረት የተገኘባቸው እርጉዝና የሚያጠቡ እናቶች ብዛት")]
    [Range(0, 10_000_000)] public int PregnantLactatingModerate { get; set; }

    [Display(Name = "MAM Rate (%)")]
    [Column(TypeName = "decimal(9,2)")]
    [Range(typeof(decimal), "0", "100")] public decimal? PregnantLactatingMamRatePercent { get; set; }

    /// <summary>GAM = (severe + moderate) ÷ screened × 100; MAM = moderate ÷ screened × 100. No one screened → no rate.</summary>
    public void CalculateRates()
    {
        ChildrenGamRatePercent = Rate(ChildrenSevere + ChildrenModerate, ChildrenScreened);
        PregnantLactatingMamRatePercent = Rate(PregnantLactatingModerate, PregnantLactatingScreened);
    }

    public static decimal? Rate(long found, long screened) =>
        screened <= 0 ? null : Math.Round(found * 100m / screened, 2, MidpointRounding.AwayFromZero);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChildrenSevere + ChildrenModerate > ChildrenScreened)
            yield return new("የስነ-ምግብ እጥረት የተገኘባቸው ህጻናት (ከፍተኛ + መካከለኛ) ልየታ ከተደረገላቸው ህጻናት መብለጥ አይችልም።",
                [nameof(ChildrenSevere), nameof(ChildrenModerate)]);
        if (PregnantLactatingModerate > PregnantLactatingScreened)
            yield return new("የስነ-ምግብ እጥረት የተገኘባቸው እናቶች ልየታ ከተደረገላቸው እናቶች መብለጥ አይችልም።",
                [nameof(PregnantLactatingModerate)]);
    }
}
