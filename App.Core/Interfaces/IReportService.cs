using App.Core.Models;

namespace App.Core.Interfaces
{
    /// <summary>
    /// Provides aggregated data for the Charting Module.
    /// </summary>
    public interface IReportService
    {
        // Pie chart — appointments grouped by status
        List<ChartDataPoint> GetAppointmentsByStatus();

        // Bar chart — appointments grouped by doctor
        List<ChartDataPoint> GetAppointmentsByDoctor();

        // Line chart — daily appointments for the last 7 days
        List<ChartDataPoint> GetDailyAppointmentsLast7Days();

        // Bar chart — appointments grouped by specialization
        List<ChartDataPoint> GetAppointmentsBySpecialization();
    }
}
