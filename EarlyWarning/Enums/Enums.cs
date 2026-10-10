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
        SubmittedToZone = 0,
        ZoneApproved = 1,
        RegionRejectedToZone = 2,
        ZoneRejectedToWoreda = 3,
        RegionApproved = 4,
        Draft = 5
    }

    public enum WeeklyWorkflowAction
    {
        Submit,
        Resubmit,
        ZoneApprove,
        ZoneReject,
        ZoneResubmit,
        RegionApprove,
        RegionReject,
        DraftSaved
    }

    public enum AnimalMigrationDirection { Inward, Outward }
    public enum MigratingAnimalType { Sheep, Goat, የቀንድ_ከብት, የጋማ_ከብት, Other }
   
    public enum Gender
    {
        ወንድ,
        ሴት
    }
   
    public enum CPDType
    {
        Pest,
        Desease
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
    public enum Measurement
    {
        ኪሎ , ሊትር, ኩንታል, ሌላ 
    }
}
