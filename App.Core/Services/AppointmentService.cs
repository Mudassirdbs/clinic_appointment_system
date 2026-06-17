using App.Core.Database;
using App.Core.Interfaces;
using App.Core.Models;
using Microsoft.Data.SqlClient;

namespace App.Core.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DatabaseHelper _db;
        public AppointmentService(DatabaseHelper db) => _db = db;

        private static Appointment Map(SqlDataReader r) => new Appointment
        {
            AppointmentId   = (int)r["AppointmentId"],
            PatientId       = (int)r["PatientId"],
            DoctorId        = (int)r["DoctorId"],
            AppointmentDate = (DateTime)r["AppointmentDate"],
            AppointmentTime = (TimeSpan)r["AppointmentTime"],
            Reason          = r["Reason"].ToString()!,
            Status          = Enum.Parse<AppointmentStatus>(r["Status"].ToString()!),
            Notes           = r["Notes"].ToString()!,
            CreatedOn       = (DateTime)r["CreatedOn"],
            PatientName     = r["PatientName"].ToString()!,
            DoctorName      = r["DoctorName"].ToString()!,
            Specialization  = r["Specialization"].ToString()!
        };

        private const string Base = @"
            SELECT a.*,p.FullName AS PatientName,
                   d.FullName AS DoctorName,d.Specialization
            FROM Appointments a
            JOIN Patients p ON a.PatientId=p.PatientId
            JOIN Doctors  d ON a.DoctorId=d.DoctorId";

        public List<Appointment> GetAllAppointments() =>
            _db.ExecuteQuery($"{Base} ORDER BY a.AppointmentDate DESC,a.AppointmentTime", Map);

        public List<Appointment> GetAppointmentsByDate(DateTime date) =>
            _db.ExecuteQuery($"{Base} WHERE CAST(a.AppointmentDate AS DATE)=@d ORDER BY a.AppointmentTime",
                Map, cmd => cmd.Parameters.AddWithValue("@d", date.Date));

        public List<Appointment> GetTodaysAppointments() => GetAppointmentsByDate(DateTime.Today);

        public List<Appointment> GetAppointmentsByPatient(int id) =>
            _db.ExecuteQuery($"{Base} WHERE a.PatientId=@id ORDER BY a.AppointmentDate DESC",
                Map, cmd => cmd.Parameters.AddWithValue("@id", id));

        public List<Appointment> GetAppointmentsByDoctor(int id) =>
            _db.ExecuteQuery($"{Base} WHERE a.DoctorId=@id ORDER BY a.AppointmentDate DESC",
                Map, cmd => cmd.Parameters.AddWithValue("@id", id));

        public Appointment? GetAppointmentById(int id) =>
            _db.ExecuteQuery($"{Base} WHERE a.AppointmentId=@id",
                Map, cmd => cmd.Parameters.AddWithValue("@id", id)).FirstOrDefault();

        public bool AddAppointment(Appointment a)
        {
            const string sql = @"INSERT INTO Appointments
                (PatientId,DoctorId,AppointmentDate,AppointmentTime,Reason,Status,Notes,CreatedOn)
                VALUES(@pat,@doc,@dt,@tm,@re,@st,@no,@cr)";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@pat", a.PatientId);
                cmd.Parameters.AddWithValue("@doc", a.DoctorId);
                cmd.Parameters.AddWithValue("@dt",  a.AppointmentDate);
                cmd.Parameters.AddWithValue("@tm",  a.AppointmentTime);
                cmd.Parameters.AddWithValue("@re",  a.Reason);
                cmd.Parameters.AddWithValue("@st",  a.Status.ToString());
                cmd.Parameters.AddWithValue("@no",  a.Notes);
                cmd.Parameters.AddWithValue("@cr",  a.CreatedOn);
            }) > 0;
        }

        public bool UpdateAppointment(Appointment a)
        {
            const string sql = @"UPDATE Appointments SET
                PatientId=@pat,DoctorId=@doc,AppointmentDate=@dt,
                AppointmentTime=@tm,Reason=@re,Status=@st,Notes=@no
                WHERE AppointmentId=@id";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@pat", a.PatientId);
                cmd.Parameters.AddWithValue("@doc", a.DoctorId);
                cmd.Parameters.AddWithValue("@dt",  a.AppointmentDate);
                cmd.Parameters.AddWithValue("@tm",  a.AppointmentTime);
                cmd.Parameters.AddWithValue("@re",  a.Reason);
                cmd.Parameters.AddWithValue("@st",  a.Status.ToString());
                cmd.Parameters.AddWithValue("@no",  a.Notes);
                cmd.Parameters.AddWithValue("@id",  a.AppointmentId);
            }) > 0;
        }

        public bool UpdateStatus(int id, AppointmentStatus status) =>
            _db.ExecuteNonQuery("UPDATE Appointments SET Status=@st WHERE AppointmentId=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@st", status.ToString());
                cmd.Parameters.AddWithValue("@id", id);
            }) > 0;

        public bool DeleteAppointment(int id) =>
            _db.ExecuteNonQuery("DELETE FROM Appointments WHERE AppointmentId=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)) > 0;

        public int GetTotalAppointmentsToday() =>
            Convert.ToInt32(_db.ExecuteScalar(
                "SELECT COUNT(*) FROM Appointments WHERE CAST(AppointmentDate AS DATE)=@t",
                cmd => cmd.Parameters.AddWithValue("@t", DateTime.Today)) ?? 0);
    }
}
