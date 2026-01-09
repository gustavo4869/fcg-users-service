using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Fcg.Users.Api.Extensions
{
    /// <summary>
    /// Extensões para facilitar o uso do Application Insights em toda a aplicação
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Registra um evento customizado no Application Insights
        /// </summary>
        /// <example>
        /// telemetryClient.TrackCustomEvent("UsuarioCriado", new { UserId = userId, Email = email });
        /// </example>
        public static void TrackCustomEvent(this TelemetryClient telemetryClient, string eventName, object? properties = null)
        {
            var eventTelemetry = new EventTelemetry(eventName);
            
            if (properties != null)
            {
                foreach (var prop in properties.GetType().GetProperties())
                {
                    var value = prop.GetValue(properties);
                    eventTelemetry.Properties[prop.Name] = value?.ToString() ?? string.Empty;
                }
            }
            
            telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Registra uma métrica customizada no Application Insights
        /// </summary>
        /// <example>
        /// telemetryClient.TrackCustomMetric("UsuariosAtivos", activeUsersCount);
        /// </example>
        public static void TrackCustomMetric(this TelemetryClient telemetryClient, string metricName, double value, IDictionary<string, string>? properties = null)
        {
            var metric = new MetricTelemetry(metricName, value);
            
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metric.Properties[prop.Key] = prop.Value;
                }
            }
            
            telemetryClient.TrackMetric(metric);
        }

        /// <summary>
        /// Registra uma operação de negócio customizada com duração
        /// </summary>
        /// <example>
        /// using (telemetryClient.StartOperation("ProcessarPagamento"))
        /// {
        ///     // código da operação
        /// }
        /// </example>
        public static IOperationHolder<RequestTelemetry> StartOperation(this TelemetryClient telemetryClient, string operationName)
        {
            return telemetryClient.StartOperation<RequestTelemetry>(operationName);
        }
    }
}
