using Hospital.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital.Shared.Services;

public interface IBloodCompatibilityService
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
}
