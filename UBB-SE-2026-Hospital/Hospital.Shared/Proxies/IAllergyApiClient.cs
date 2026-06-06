using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IAllergyApiClient
{
    Task<List<Allergy>> GetAllergiesAsync(CancellationToken cancellationToken = default);
}


