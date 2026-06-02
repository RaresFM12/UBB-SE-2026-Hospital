using Hospital.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital.Shared.Services;

public interface IAllergyService
{
    Task<List<Allergy>> GetAllAsync();
}
