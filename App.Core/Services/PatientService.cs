using App.Core.Database;
using App.Core.Interfaces;
using App.Core.Models;
using Microsoft.Data.SqlClient;

namespace App.Core.Services
{
    public class PatientService : IPatientService
    {
        private readonly DatabaseHelper _db;
        public PatientService(DatabaseHelper db) => _db = db;

        private static Patient Map(SqlDataReader r) => new Patient
        {
            PatientId    = (int)r["PatientId"],
            FullName     = r["FullName"].ToString()!,
            Gender       = r["Gender"].ToString()!,
            DateOfBirth  = (DateTime)r["DateOfBirth"],
            PhoneNumber  = r["PhoneNumber"].ToString()!,
            Email        = r["Email"].ToString()!,
            Address      = r["Address"].ToString()!,
            BloodGroup   = r["BloodGroup"].ToString()!,
            RegisteredOn = (DateTime)r["RegisteredOn"]
        };

        public List<Patient> GetAllPatients() =>
            _db.ExecuteQuery("SELECT * FROM Patients ORDER BY FullName", Map);

        // Async implementation — bonus mark criterion
        public async Task<List<Patient>> GetAllPatientsAsync() =>
            await _db.ExecuteQueryAsync("SELECT * FROM Patients ORDER BY FullName", Map);

        public Patient? GetPatientById(int id) =>
            _db.ExecuteQuery("SELECT * FROM Patients WHERE PatientId=@id", Map,
                cmd => cmd.Parameters.AddWithValue("@id", id)).FirstOrDefault();

        public List<Patient> SearchPatients(string keyword) =>
            _db.ExecuteQuery(
                "SELECT * FROM Patients WHERE FullName LIKE @kw OR PhoneNumber LIKE @kw OR BloodGroup LIKE @kw ORDER BY FullName",
                Map, cmd => cmd.Parameters.AddWithValue("@kw", $"%{keyword}%"));

        public bool AddPatient(Patient p)
        {
            const string sql = @"INSERT INTO Patients
                (FullName,Gender,DateOfBirth,PhoneNumber,Email,Address,BloodGroup,RegisteredOn)
                VALUES(@n,@g,@d,@ph,@em,@ad,@bl,@ro)";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@n",  p.FullName);
                cmd.Parameters.AddWithValue("@g",  p.Gender);
                cmd.Parameters.AddWithValue("@d",  p.DateOfBirth);
                cmd.Parameters.AddWithValue("@ph", p.PhoneNumber);
                cmd.Parameters.AddWithValue("@em", p.Email);
                cmd.Parameters.AddWithValue("@ad", p.Address);
                cmd.Parameters.AddWithValue("@bl", p.BloodGroup);
                cmd.Parameters.AddWithValue("@ro", p.RegisteredOn);
            }) > 0;
        }

        public bool UpdatePatient(Patient p)
        {
            const string sql = @"UPDATE Patients SET
                FullName=@n,Gender=@g,DateOfBirth=@d,
                PhoneNumber=@ph,Email=@em,Address=@ad,BloodGroup=@bl
                WHERE PatientId=@id";
            return _db.ExecuteNonQuery(sql, cmd =>
            {
                cmd.Parameters.AddWithValue("@n",  p.FullName);
                cmd.Parameters.AddWithValue("@g",  p.Gender);
                cmd.Parameters.AddWithValue("@d",  p.DateOfBirth);
                cmd.Parameters.AddWithValue("@ph", p.PhoneNumber);
                cmd.Parameters.AddWithValue("@em", p.Email);
                cmd.Parameters.AddWithValue("@ad", p.Address);
                cmd.Parameters.AddWithValue("@bl", p.BloodGroup);
                cmd.Parameters.AddWithValue("@id", p.PatientId);
            }) > 0;
        }

        public bool DeletePatient(int id)
        {
            // Remove related appointments first (referential integrity)
            _db.ExecuteNonQuery("DELETE FROM Appointments WHERE PatientId=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id));
            return _db.ExecuteNonQuery("DELETE FROM Patients WHERE PatientId=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)) > 0;
        }

        public int GetTotalPatients() =>
            Convert.ToInt32(_db.ExecuteScalar("SELECT COUNT(*) FROM Patients") ?? 0);
    }
}
