using EarlyWarning.Models.AnimalPrice;

namespace EarlyWarning.ViewModels.GrainViewModel
{
    public class AnimalPriceReportViewModel
    {
        public AnimalPricePerUnit? AnimalPricePerUnit { get; set; }
        public decimal? PreviousPrice { get; set; }
        public decimal? PriceDifference { get; set; }
        public decimal? PriceChangePercentage { get; set; }

        // Computed properties for display
        public string TrendClass
        {
            get
            {
                if (!PriceChangePercentage.HasValue) return "trend-same";
                return PriceChangePercentage.Value > 0 ? "trend-up" :
                       PriceChangePercentage.Value < 0 ? "trend-down" : "trend-same";
            }
        }

        public string TrendIcon
        {
            get
            {
                if (!PriceChangePercentage.HasValue) return "➡️";
                return PriceChangePercentage.Value > 0 ? "📈" :
                       PriceChangePercentage.Value < 0 ? "📉" : "➡️";
            }
        }

        public string PercentageDisplay
        {
            get
            {
                if (!PriceChangePercentage.HasValue) return "-";
                if (PriceChangePercentage.Value > 0)
                    return $"+{PriceChangePercentage.Value:F2}%";
                return $"{PriceChangePercentage.Value:F2}%";
            }
        }

        public string DifferenceDisplay
        {
            get
            {
                if (!PriceDifference.HasValue) return "-";
                if (PriceDifference.Value > 0)
                    return $"+{PriceDifference.Value:N2} ብር";
                return $"{PriceDifference.Value:N2} ብር";
            }
        }
    }
}
