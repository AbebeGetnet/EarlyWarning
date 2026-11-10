using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Enums
{
    public enum ApprovalStatus
    {
        በሂደት_ላይ,
        የተፈቀደ,
        የተከለከለ
    }

    public enum CropGrowthType
    {
        SeedLevel,          
        GerminationLevel,      
        GrowthLevel,
        SeedFillingLevel,
        HarvestingLevel
    }
    public enum ReportStatus
    {
        Draft,          
        Submitted,      
        ZoneApproved,  
        RegionApproved,
        Rejected
    }

    public enum WeeklySubmissionStatus
    {
        [Display(Name = "ለዞን ተልኳል")] SubmittedToZone = 0,
        [Display(Name = "በዞን ጸድቋል")] ZoneApproved = 1,
        [Display(Name = "በክልል ተመልሷል")] RegionRejectedToZone = 2,
        [Display(Name = "ወደ ወረዳ ተመልሷል")] ZoneRejectedToWoreda = 3,
        [Display(Name = "በክልል ጸድቋል")] RegionApproved = 4,
        [Display(Name = "ረቂቅ")] Draft = 5
    }

    public enum WeeklyWorkflowAction
    {
        [Display(Name = "ለዞን ተልኳል")] Submit,
        [Display(Name = "ተስተካክሎ እንደገና ተልኳል")] Resubmit,
        [Display(Name = "በዞን ጸድቆ ለክልል ተልኳል")] ZoneApprove,
        [Display(Name = "በዞን ውድቅ ተደርጓል")] ZoneReject,
        [Display(Name = "በዞን ተስተካክሎ ለክልል ተልኳል")] ZoneResubmit,
        [Display(Name = "በክልል ጸድቋል")] RegionApprove,
        [Display(Name = "በክልል ውድቅ ተደርጓል")] RegionReject,
        [Display(Name = "ረቂቅ ተቀምጧል")] DraftSaved
    }

    public enum AnimalMigrationDirection { Inward, Outward }
    public enum MigratingAnimalType { [Display(Name = "በግ")] Sheep, [Display(Name = "ፍየል")] Goat, የቀንድ_ከብት, የጋማ_ከብት, [Display(Name = "ሌላ")] Other }
   
    public enum Gender
    {
        ወንድ,
        ሴት
    }
   
    public enum CPDType
    {
        [Display(Name = "ተባይ")] Pest,
        [Display(Name = "በሽታ")] Desease
    }

    public enum Months
    {
        መስከረም,
        ጥቅምት,
        ህዳር,
        ታህሳስ,
        ጥር,
        የካቲት,
        መጋቢት,
        ሚያዚያ,
        ግቦት,
        ሰኔ,
        ሀምሌ,
        ነሃሴ
    }
    
   
    public enum LocationLevel
    {
        ሀገር,
        ክልል,
        ዞን,
        ወረዳ,
        ቀበሌ
    }
    public enum AnimalBodyCondition
    {
        መካከለኛ = 1,
        ዝቅተኛ = 2
    }
    public enum AidCategory
    {
        [Display(Name = "የምግብ እርዳታ")] Food = 1,
        [Display(Name = "የምግብ ያልሆነ እርዳታ")] NonFood = 2
    }

    // What a natural or human-made danger damaged; several can apply to one danger.
    [Flags]
    public enum AccidentDamageCategory
    {
        None = 0,
        [Display(Name = "ሰው")] Human = 1,
        [Display(Name = "እንስሳት")] Animal = 2,
        [Display(Name = "ቤት")] House = 4,
        [Display(Name = "ሰብል/ማሳ")] Crop = 8,
        [Display(Name = "መሰረተ ልማት")] Infrastructure = 16,
        [Display(Name = "ሌላ")] Other = 32
    }

    public enum Measurement
    {
        ኪሎ , ሊትር, ኩንታል, ሌላ 
    }
}
