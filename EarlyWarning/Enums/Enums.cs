using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EarlyWarning.Enums
{
    public enum ApprovalStatus
    {
        በሂደት_ላይ,
        የተፈቀደ,
        የተከለከለ,
    }
    public enum Job
    {
        ፈላጊ,
        የመንግስት,
        የግል,
        ተማሪ,
        የቤት_እመቤት, ጦሮታ
    }
    public enum Gender
    {
        ወንድ,
        ሴት
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
