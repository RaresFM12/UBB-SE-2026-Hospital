namespace Hospital.Shared.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hospital.Shared.Models;

    public interface ISalaryComputationService
    {
        Task<double> ComputeSalaryDoctorAsync(Doctor doctor, List<Shift> monthlyShifts, int month, int year);

        Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, List<Shift> monthlyShifts, int month, int year);

        List<IStaff> GetAllStaff();

        List<Shift> GetAllShifts();
    }
}
