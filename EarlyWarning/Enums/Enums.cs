using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Enums
{
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
}
