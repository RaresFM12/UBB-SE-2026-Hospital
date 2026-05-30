using System.Collections.Generic;

namespace Hospital.Data.Models.DTOs;

public static class MockDoctorProvider
{
    public static List<DoctorDetails> FakeDoctors
        => new List<DoctorDetails>
        {
            new DoctorDetails { DoctorId = 1, FirstName = "Gregory", LastName = "House", Specialization = "Diagnostician", },
            new DoctorDetails { DoctorId = 2, FirstName = "James", LastName = "Wilson", Specialization = "Oncology", },
            new DoctorDetails { DoctorId = 3, FirstName = "Lisa", LastName = "Cuddy", Specialization = "Endocrinology", },
            new DoctorDetails { DoctorId = 4, FirstName = "Meredith", LastName = "Grey", Specialization = "General Surgery", }
        };
}
