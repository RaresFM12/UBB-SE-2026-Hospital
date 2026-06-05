using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;

namespace Hospital.Services;

public class AllergyService(IAllergyRepository allergyRepository) : IAllergyService
{
    public Task<List<Allergy>> GetAllergiesAsync()
        => allergyRepository.GetAllAsync();
}
