using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class SalaryComputationApiClient(HttpClient httpClient) : ApiClientBase(httpClient), ISalaryComputationService, ISalaryComputationApiClient
{
    private const string BaseUri = "api/salaries";
    private const string StaffUri = "api/staff";
    private const string ShiftsUri = "api/shifts";

    public async Task<double> ComputeSalaryDoctorAsync(Doctor doctor, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
        => await PostAsync<object, double>($"{BaseUri}/doctor", new { doctor, monthlyShifts, month, year }, cancellationToken);

    public async Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, IReadOnlyList<Shift> monthlyShifts, int month, int year, CancellationToken cancellationToken = default)
        => await PostAsync<object, double>($"{BaseUri}/pharmacist", new { pharmacist, monthlyShifts, month, year }, cancellationToken);

    public async Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Staff>>(StaffUri, cancellationToken) ?? [];

    public async Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>(ShiftsUri, cancellationToken) ?? [];
}
