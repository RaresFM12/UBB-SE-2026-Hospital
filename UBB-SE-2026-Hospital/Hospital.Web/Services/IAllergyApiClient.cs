using Hospital.Data.Models;

namespace Hospital.Web.Services;

public interface IAllergyApiClient
{
    Task<List<Allergy>> GetAllergiesAsync(CancellationToken cancellationToken = default);
    Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default);
}
