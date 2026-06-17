using App.Core.Database;
using App.Core.Interfaces;
using App.Core.Models;
using Microsoft.Data.SqlClient;

namespace App.Core.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly DatabaseHelper _db;
        public DoctorService(DatabaseHelper db) => _db = db;

        private static Doctor Map(SqlDataReader r) => new Doctor
        {
            DoctorId        = (int)r["DoctorId"],
            FullName        = r["FullName"].ToString()!,
            Specialization  = r["Specialization"].ToString()!,
            PhoneNumber     = r["PhoneNumber"].ToString()!,
            Email           = r["Email"].ToString()!,
            Qualification   = r["Qualification"].ToString()!,
            AvailableDays   = r["AvailableDays"].ToString()!,
            StartTime       = (TimeSpan)r["StartTime"],
            EndTime         = (TimeSpan)r["EndTime"],
            ConsultationFee = (decimal)r["ConsultationFee"],
            IsActive        = (bool)r["IsActive"]
        };

        public List<Doctor> GetAllDoctors() =>
            _db.ExecuteQuery("SELECT * FROM Doctors ORDER BY FullName", Map);

        public List<Doctor> GetActiveDoctors() =>
            _db.ExecuteQuery("SELECT * FROM Doctors WHERE IsActive=1 ORDER BY FullName", Map);

        public Doctor? GetDoctorById(int id) =>
            _db.ExecuteQuery("SELECT * FROM Doctors WHERE DoctorId=@id", Map,
                cmd => cmd.Parameters.AddWithValue("@id", id)).FirstOrDefault();

        public bool AddDoctor(Doctor d)
        {
            const string sql = @"INSERT INTO Doctors
                (FullName,Specialization,PhoneNumber,Email,Qualification,
                 AvailableDays,StartTime,EndTime,ConsultationFee,IsActive)
                VALUES(@n,@sp,@ph,@em,@qu,@dy,@st,@en,@fe,@ac)";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@n",  d.FullName);
                cmd.Parameters.AddWithValue("@sp", d.Specialization);
                cmd.Parameters.AddWithValue("@ph", d.PhoneNumber);
                cmd.Parameters.AddWithValue("@em", d.Email);
                cmd.Parameters.AddWithValue("@qu", d.Qualification);
                cmd.Parameters.AddWithValue("@dy", d.AvailableDays);
                cmd.Parameters.AddWithValue("@st", d.StartTime);
                cmd.Parameters.AddWithValue("@en", d.EndTime);
                cmd.Parameters.AddWithValue("@fe", d.ConsultationFee);
                cmd.Parameters.AddWithValue("@ac", d.IsActive);
            }) > 0;
        }

        public bool UpdateDoctor(Doctor d)
        {
            const string sql = @"UPDATE Doctors SET
                FullName=@n,Specialization=@sp,PhoneNumber=@ph,Email=@em,
                Qualification=@qu,AvailableDays=@dy,StartTime=@st,
                EndTime=@en,ConsultationFee=@fe,IsActive=@ac
                WHERE DoctorId=@id";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@n",  d.FullName);
                cmd.Parameters.AddWithValue("@sp", d.Specialization);
                cmd.Parameters.AddWithValue("@ph", d.PhoneNumber);
                cmd.Parameters.AddWithValue("@em", d.Email);
                cmd.Parameters.AddWithValue("@qu", d.Qualification);
                cmd.Parameters.AddWithValue("@dy", d.AvailableDays);
                cmd.Parameters.AddWithValue("@st", d.StartTime);
                cmd.Parameters.AddWithValue("@en", d.EndTime);
                cmd.Parameters.AddWithValue("@fe", d.ConsultationFee);
                cmd.Parameters.AddWithValue("@ac", d.IsActive);
                cmd.Parameters.AddWithValue("@id", d.DoctorId);
            }) > 0;
        }

        public bool DeleteDoctor(int id)
        {
            _db.ExecuteNonQuery("DELETE FROM Appointments WHERE DoctorId=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id));
            return _db.ExecuteNonQuery("DELETE FROM Doctors WHERE DoctorId=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)) > 0;
        }

        public int GetTotalDoctors() =>
            Convert.ToInt32(_db.ExecuteScalar("SELECT COUNT(*) FROM Doctors WHERE IsActive=1") ?? 0);
    }
}
