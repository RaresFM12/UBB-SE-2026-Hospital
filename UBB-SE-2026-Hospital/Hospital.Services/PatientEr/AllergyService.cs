using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;

namespace Hospital.Services.PatientEr;

public class AllergyService(IAllergyRepository allergyRepository) :
    IAllergyService,
    Hospital.Shared.Services.IAllergyService
{
    public Task<List<Allergy>> GetAllergiesAsync()
        => allergyRepository.GetAllAsync();

    public Task<List<Allergy>> GetAllAsync()
        => allergyRepository.GetAllAsync();
}
