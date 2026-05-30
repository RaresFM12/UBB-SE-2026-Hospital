using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class AllergyRepository(HospitalDbContext context) : IAllergyRepository
{
    public async Task<Allergy?> GetByIdAsync(int allergyId)
        => await context.Allergies.FindAsync(allergyId);

    public async Task<List<Allergy>> GetAllAsync()
        => await context.Allergies.ToListAsync();

    public async Task<Allergy> CreateAsync(Allergy allergy)
    {
        context.Allergies.Add(allergy);
        await context.SaveChangesAsync();
        return allergy;
    }

    public async Task DeleteAsync(int allergyId)
    {
        var allergy = await context.Allergies.FindAsync(allergyId);
        if (allergy is not null)
        {
            context.Allergies.Remove(allergy);
            await context.SaveChangesAsync();
        }
    }
}
