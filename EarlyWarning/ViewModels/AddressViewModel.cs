using EarlyWarning.Enums;
using EarlyWarning.Models;
using System.ComponentModel;
using System.Security.Policy;

namespace EarlyWarning.ViewModels.AddressViewModel
{
    public class AddressViewModel
    {
        public Locations Region { get; set; }
        public Locations Zone { get; set; }
        public Locations Woreda { get; set; }
        public Locations Kebelie { get; set; }
        public List<Locations> Regions { get; set; }
        public List<Locations> Zones { get; set; }
        public List<Locations> Woreds { get; set; }
        public List<Locations> Kebelies { get; set; }

        public Guid Id { get; set; }
        [DisplayName("location Name")]
        public string LocationName { get; set; }
        [DisplayName("የቦታው ስም")]
        public string LocationAmharicName { get; set; }
        [DisplayName("አጭር ስም")]
        public string AbrevatedName { get; set; }
        [DisplayName("የአድራሻ ኮድ")]
        public string LocationCode { get; set; }
        [DisplayName("ስልክ")]
        public string PhoneNumber { get; set; }   
        public LocationLevel Level { get; set; }
        [DisplayName("ሔደር")]
        public string? CardHeaderTitle { get; set; }
        public Guid? ParentId { get; set; }
    }
}
