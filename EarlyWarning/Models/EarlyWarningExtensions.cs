using System;
using EarlyWarning.Models;
using EarlyWarning.ViewModels;
using EarlyWarning.Enums;

namespace EarlyWarning.Extensions
{
    public static class EarlyWarningExtensions
    {
        /// <summary>
        /// Binds shared wizard metadata down to an individual early warning sub-report entity.
        /// </summary>
        public static T BindSharedProperties<T>(
            this T report,
            RegistrationWithardViewModel wizardModel,
            string currentUserId,
            DateTime timeStamp) where T : CommonAttribute
        {
            if (report == null) throw new ArgumentNullException(nameof(report));

            report.Id = Guid.NewGuid();
            report.StartDate = wizardModel.StartDate;
            report.EndDate = wizardModel.EndDate;
            report.UserId = currentUserId;
            report.SubmittedById = currentUserId;
            report.LasModifiedAt = timeStamp;
            report.Status = ReportStatus.Draft;

            // Use reflection to safely assign the required WoredaId field dynamically
            var woredaIdProperty = typeof(T).GetProperty("WoredaId");
            if (woredaIdProperty != null && woredaIdProperty.CanWrite)
            {
                woredaIdProperty.SetValue(report, wizardModel.WoredaId);
            }

            return report;
        }
    }
}