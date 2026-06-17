using App.Core.Database;
using App.Core.Interfaces;
using App.Core.Models;
using Microsoft.Data.SqlClient;

namespace App.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly DatabaseHelper _db;
        public ReportService(DatabaseHelper db) => _db = db;

        /// <summary>Pie chart — count of appointments grouped by Status.</summary>
        public List<ChartDataPoint> GetAppointmentsByStatus() =>
            _db.ExecuteQuery(
                "SELECT Status, COUNT(*) AS Total FROM Appointments GROUP BY Status",
                r => new ChartDataPoint(r["Status"].ToString()!, Convert.ToDouble(r["Total"])));

        /// <summary>Bar chart — count of appointments per doctor name.</summary>
        public List<ChartDataPoint> GetAppointmentsByDoctor() =>
            _db.ExecuteQuery(@"
                SELECT d.FullName AS DoctorName, COUNT(*) AS Total
                FROM Appointments a
                JOIN Doctors d ON a.DoctorId = d.DoctorId
                GROUP BY d.FullName
                ORDER BY Total DESC",
                r => new ChartDataPoint(r["DoctorName"].ToString()!, Convert.ToDouble(r["Total"])));

        /// <summary>Line chart — daily appointment counts for the last 7 days.</summary>
        public List<ChartDataPoint> GetDailyAppointmentsLast7Days() =>
            _db.ExecuteQuery(@"
                SELECT CAST(AppointmentDate AS DATE) AS Day, COUNT(*) AS Total
                FROM Appointments
                WHERE AppointmentDate >= DATEADD(DAY,-6,CAST(GETDATE() AS DATE))
                GROUP BY CAST(AppointmentDate AS DATE)
                ORDER BY Day",
                r => new ChartDataPoint(
                    Convert.ToDateTime(r["Day"]).ToString("dd MMM"),
                    Convert.ToDouble(r["Total"])));

        /// <summary>Bar chart — appointments grouped by doctor specialization.</summary>
        public List<ChartDataPoint> GetAppointmentsBySpecialization() =>
            _db.ExecuteQuery(@"
                SELECT d.Specialization, COUNT(*) AS Total
                FROM Appointments a
                JOIN Doctors d ON a.DoctorId = d.DoctorId
                GROUP BY d.Specialization
                ORDER BY Total DESC",
                r => new ChartDataPoint(r["Specialization"].ToString()!, Convert.ToDouble(r["Total"])));
    }
}
