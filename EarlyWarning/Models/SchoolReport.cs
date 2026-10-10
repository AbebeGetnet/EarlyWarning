using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Models;

public class SchoolReport : IValidatableObject
{
    public Guid SchoolReportId { get; set; } = Guid.NewGuid();
    public Guid WoredaId { get; set; }
    [ForeignKey(nameof(WoredaId))] public Locations? Woreda { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Display(Name = "በትምህርትና መማር ሂደት ላይ ችግር ነበር?")]
    public bool HasTeachingLearningProblem { get; set; }
    [StringLength(1000)] public string? Cause { get; set; }
    [Range(0, 1_000_000)] public int MaleStudentDropouts { get; set; }
    [Range(0, 1_000_000)] public int FemaleStudentDropouts { get; set; }
    [Display(Name = "የማቋረጥ ጥምርታ/ምጣኔ በ%")]
    [Range(typeof(decimal), "0", "100")] public decimal? DropoutRatePercent { get; set; }
    [Range(typeof(decimal), "0", "100")] public decimal? AverageDropoutAge { get; set; }
    [Range(0, 100_000)] public int ClosedSchools { get; set; }
    [StringLength(500)] public string? MajorSolutionOne { get; set; }
    [StringLength(500)] public string? MajorSolutionTwo { get; set; }
    [StringLength(500)] public string? MajorSolutionThree { get; set; }
    public bool? WasActionSatisfactory { get; set; }

    public bool StudentRelatedSolution { get; set; }
    public bool TeacherExpenseSolution { get; set; }
    public bool ClassroomMaterialsSolution { get; set; }
    public bool SchoolBuildingServiceSolution { get; set; }
    public bool OtherSolution { get; set; }
    [StringLength(500)] public string? OtherSolutionDescription { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasTeachingLearningProblem) yield break;
        if (string.IsNullOrWhiteSpace(Cause)) yield return new("የችግሩ ምክንያት ግዴታ ነው።", [nameof(Cause)]);
        if (string.IsNullOrWhiteSpace(MajorSolutionOne) || string.IsNullOrWhiteSpace(MajorSolutionTwo) || string.IsNullOrWhiteSpace(MajorSolutionThree))
            yield return new("ሶስቱንም ዋና ዋና መፍትሄዎች ያስገቡ።", [nameof(MajorSolutionOne), nameof(MajorSolutionTwo), nameof(MajorSolutionThree)]);
        if (WasActionSatisfactory == null) yield return new("የተወሰደው እርምጃ አጥጋቢ መሆኑን ይምረጡ።", [nameof(WasActionSatisfactory)]);
        if (!(StudentRelatedSolution || TeacherExpenseSolution || ClassroomMaterialsSolution || SchoolBuildingServiceSolution || OtherSolution))
            yield return new("ቢያንስ አንድ የመፍትሄ ምድብ ይምረጡ።", [nameof(StudentRelatedSolution)]);
        if (OtherSolution && string.IsNullOrWhiteSpace(OtherSolutionDescription))
            yield return new("ሌላ መፍትሄ ሲመረጥ ማብራሪያ ያስፈልጋል።", [nameof(OtherSolutionDescription)]);
        if (MaleStudentDropouts + FemaleStudentDropouts > 0 && AverageDropoutAge == null)
            yield return new("ከትምህርት የወጡ ተማሪዎች ካሉ አማካይ ዕድሜ ያስፈልጋል።", [nameof(AverageDropoutAge)]);
    }
}
