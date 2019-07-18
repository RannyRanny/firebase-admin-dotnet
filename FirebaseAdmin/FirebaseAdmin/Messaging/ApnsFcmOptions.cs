using FirebaseAdmin.Messaging.Util;
using Newtonsoft.Json;

namespace FirebaseAdmin.Messaging
{
    /// <summary>
    /// Represents Apple Push Notification Service FCM options.
    /// </summary>
    public sealed class ApnsFcmOptions
    {
        /// <summary>
        /// Gets or sets analytics label.
        /// </summary>
        [JsonProperty("analytics_label")]
        public string AnalyticsLabel { get; set; }

        /// <summary>
        /// Copies this FCM options, and validates the content of it to ensure that it can
        /// be serialized into the JSON format expected by the FCM service.
        /// </summary>
        internal ApnsFcmOptions CopyAndValidate()
        {
            var copy = new ApnsFcmOptions()
            {
                AnalyticsLabel = this.AnalyticsLabel,
            };
            AnalyticsLabelChecker.CheckAnalyticsLabelOrThrow(this.AnalyticsLabel);

            return copy;
        }
    }
}