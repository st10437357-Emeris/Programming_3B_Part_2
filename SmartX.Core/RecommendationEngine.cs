using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartX.Core.Intelligence
{
    public record ActionInsight(
        string InsightType,    // e.g., "Suggested Action" or "Automated Insight"
        string DeviceId,
        string RecommendedAction,
        string Reason,
        DateTime Timestamp
    );

    public record TelemetryEvent(
        string DeviceId,
        string MetricName,
        double Value,
        DateTime Timestamp
    );

    public class RecommendationEngine
    {
        // Tracks correlations: Condition Pattern Key -> Triggered Action Count
        // e.g., "Moisture_Low" -> {"Trigger_Pump_A": 8, "Inspect_Valve": 2}
        private readonly Dictionary<string, Dictionary<string, int>> _learnedCorrelations;

        // Stores recent anomalies or problematic threshold states
        private readonly List<TelemetryEvent> _recentTelemetryEvents;

        public RecommendationEngine()
        {
            _learnedCorrelations = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
            _recentTelemetryEvents = new List<TelemetryEvent>();
        }

        /// <summary>
        /// Records when an operator manually searches for or executes an action following an event.
        /// Trains the pattern matching model over time.
        /// </summary>
        public void LearnUserBehavior(string triggerPattern, string executedAction)
        {
            if (!_learnedCorrelations.ContainsKey(triggerPattern))
            {
                _learnedCorrelations[triggerPattern] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            var actionMap = _learnedCorrelations[triggerPattern];
            if (!actionMap.TryAdd(executedAction, 1))
            {
                actionMap[executedAction]++;
            }
        }

        /// <summary>
        /// Ingests incoming telemetry to check against operational thresholds.
        /// </summary>
        public void IngestEvent(TelemetryEvent telemetry)
        {
            _recentTelemetryEvents.Add(telemetry);

            // Keep the rolling window manageable (e.g., last 100 events)
            if (_recentTelemetryEvents.Count > 100)
            {
                _recentTelemetryEvents.RemoveAt(0);
            }
        }

        /// <summary>
        /// Evaluates active conditions and returns actionable recommendations for the dashboard.
        /// </summary>
        public List<ActionInsight> GenerateRecommendations()
        {
            var insights = new List<ActionInsight>();

            foreach (var evt in _recentTelemetryEvents)
            {
                // 1. Proactive Fault / Out-of-Range Detection (Automated Insights)
                if (evt.MetricName.Equals("Moisture", StringComparison.OrdinalIgnoreCase) && evt.Value < 20.0)
                {
                    insights.Add(new ActionInsight(
                        InsightType: "Automated Insight",
                        DeviceId: evt.DeviceId,
                        RecommendedAction: "Inspect Irrigation Line",
                        Reason: $"Critical moisture drop detected ({evt.Value}%). Potential dry-run failure.",
                        Timestamp: DateTime.UtcNow
                    ));

                    // 2. Behavioral Pattern Suggestion (Suggested Actions)
                    const string patternKey = "Moisture_Low";
                    if (_learnedCorrelations.TryGetValue(patternKey, out var suggestedActions))
                    {
                        var topAction = suggestedActions.OrderByDescending(kv => kv.Value).FirstOrDefault();
                        if (topAction.Key != null)
                        {
                            insights.Add(new ActionInsight(
                                InsightType: "Suggested Action",
                                DeviceId: evt.DeviceId,
                                RecommendedAction: topAction.Key,
                                Reason: $"Operators triggered '{topAction.Key}' {topAction.Value} time(s) during past low moisture states.",
                                Timestamp: DateTime.UtcNow
                            ));
                        }
                    }
                }
                else if (evt.MetricName.Equals("PowerWatts", StringComparison.OrdinalIgnoreCase) && evt.Value > 3500.0)
                {
                    insights.Add(new ActionInsight(
                        InsightType: "Automated Insight",
                        DeviceId: evt.DeviceId,
                        RecommendedAction: "Trip Smart Breaker",
                        Reason: $"High power surge ({evt.Value}W) exceeds safe threshold.",
                        Timestamp: DateTime.UtcNow
                    ));
                }
            }

            return insights;
        }
    }
}