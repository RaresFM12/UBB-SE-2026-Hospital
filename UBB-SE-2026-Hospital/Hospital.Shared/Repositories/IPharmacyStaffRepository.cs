namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IPharmacyStaffRepository
    {
        List<Pharmacyst> GetPharmacists();
    }
}
