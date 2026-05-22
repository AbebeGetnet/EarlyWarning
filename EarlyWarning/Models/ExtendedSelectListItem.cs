using Microsoft.AspNetCore.Mvc.Rendering;

namespace EarlyWarning.ViewModels
{
    public class ExtendedSelectListItem : SelectListItem
    {
        public int KebeleCount { get; set; }

        public ExtendedSelectListItem() : base() { }

        public ExtendedSelectListItem(string text, string value, int kebeleCount)
            : base(text, value)
        {
            KebeleCount = kebeleCount;
        }
    }
}