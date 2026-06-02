using Hospital.Data.Models;

namespace Hospital.Services.PatientEr;

public interface ITransplantService
{
    Task<List<Transplant>> GetAllAsync();
    Task<Transplant?> GetByIdAsync(int id);
    Task<Transplant> CreateAsync(Transplant transplant);
    Task<Transplant> UpdateAsync(Transplant transplant);
    Task DeleteAsync(int id);
    Task<List<Transplant>> GetByPatientIdAsync(int patientId);
    Task<List<Transplant>> GetByReceiverIdAsync(int receiverId);
    Task<List<Transplant>> GetByDonorIdAsync(int donorId);
    Task<List<TransplantMatch>> GetMatchesAsync();
    Task CreateWaitlistRequestAsync(int receiverId, string organType);
    Task AssignDonorAsync(int transplantId, int donorId, float finalScore);
    Task<List<TransplantMatch>> GetTopMatchesAsDisplayModelsAsync(int donorId, string organType);
    Task<bool> IsUrgentAsync(int patientId);
    Task<string?> GetChronicWarningAsync(int patientId);
}
