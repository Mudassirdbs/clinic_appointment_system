using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IPatientService
    {
        List<Patient> GetAllPatients();
        Task<List<Patient>> GetAllPatientsAsync();   // async — bonus mark
        Patient? GetPatientById(int id);
        List<Patient> SearchPatients(string keyword);
        bool AddPatient(Patient patient);
        bool UpdatePatient(Patient patient);
        bool DeletePatient(int id);
        int GetTotalPatients();
    }
}
