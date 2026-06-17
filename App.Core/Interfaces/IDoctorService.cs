using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IDoctorService
    {
        List<Doctor> GetAllDoctors();
        List<Doctor> GetActiveDoctors();
        Doctor? GetDoctorById(int id);
        bool AddDoctor(Doctor doctor);
        bool UpdateDoctor(Doctor doctor);
        bool DeleteDoctor(int id);
        int GetTotalDoctors();
    }
}
