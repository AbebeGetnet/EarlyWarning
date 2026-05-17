using System.ComponentModel.DataAnnotations;

namespace EarlyWarning.Extensions
{
    public static class DisplayNameExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
                return value.ToString();

            var attributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                var displayAttribute = (DisplayAttribute)attributes[0];
                return displayAttribute.GetName() ?? value.ToString();
            }

            return value.ToString();
        }
    }
}
