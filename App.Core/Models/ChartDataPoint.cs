namespace App.Core.Models
{
    /// <summary>
    /// Represents a single data point used for chart rendering.
    /// </summary>
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }

        public ChartDataPoint() { }
        public ChartDataPoint(string label, double value)
        {
            Label = label;
            Value = value;
        }
    }
}
