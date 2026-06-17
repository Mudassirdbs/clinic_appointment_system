using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IAppointmentService
    {
        List<Appointment> GetAllAppointments();
        List<Appointment> GetAppointmentsByDate(DateTime date);
        List<Appointment> GetAppointmentsByPatient(int patientId);
        List<Appointment> GetAppointmentsByDoctor(int doctorId);
        List<Appointment> GetTodaysAppointments();
        Appointment? GetAppointmentById(int id);
        bool AddAppointment(Appointment appointment);
        bool UpdateAppointment(Appointment appointment);
        bool UpdateStatus(int appointmentId, AppointmentStatus status);
        bool DeleteAppointment(int id);
        int GetTotalAppointmentsToday();
    }
}
