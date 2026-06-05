using Hospital.Data.Models;

namespace Hospital.Web.Models
{
    public class DoctorScheduleViewModel
    {
        public List<Shift> Shifts { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }
}
