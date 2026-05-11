using EarlyWarning.Enums;

namespace EarlyWarning.Models
{
    public class CropPestAndDesease
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CPDType CPDType { get; set; }
    }
}
