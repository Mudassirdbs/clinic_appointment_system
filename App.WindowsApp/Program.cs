using App.Core.Database;
using App.Core.Services;
using App.WindowsApp.Forms;

namespace App.WindowsApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ── CONNECTION STRING ─────────────────────────────────────────────
            // Change this to match your SQL Server instance:
            //   LocalDB  : Server=(localdb)\\MSSQLLocalDB;Database=ClinicDB;Trusted_Connection=True;
            //   Express  : Server=.\\SQLEXPRESS;Database=ClinicDB;Trusted_Connection=True;TrustServerCertificate=True;
            //   Default  : Server=.;Database=ClinicDB;Trusted_Connection=True;TrustServerCertificate=True;
            const string connectionString =
                "Server=(localdb)\\MSSQLLocalDB;Database=ClinicDB;Trusted_Connection=True;";

            var db                 = new DatabaseHelper(connectionString);
            var patientService     = new PatientService(db);
            var doctorService      = new DoctorService(db);
            var appointmentService = new AppointmentService(db);
            var reportService      = new ReportService(db);

            if (!db.TestConnection())
            {
                MessageBox.Show(
                    "Cannot connect to the database.\n\n" +
                    "1. Make sure SQL Server / LocalDB is running.\n" +
                    "2. Run Database\\clinic_db.sql in SSMS first.\n" +
                    "3. Check the connection string in Program.cs.\n\n" +
                    "Current connection string:\n" + connectionString,
                    "Database Connection Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MainForm(patientService, doctorService, appointmentService, reportService));
        }
    }
}
