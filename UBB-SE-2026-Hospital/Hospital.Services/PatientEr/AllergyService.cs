using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;

namespace Hospital.Services.PatientEr;

public class AllergyService(IAllergyRepository allergyRepository) : IAllergyService
{
    public Task<List<Allergy>> GetAllergiesAsync()
        => allergyRepository.GetAllAsync();
}
