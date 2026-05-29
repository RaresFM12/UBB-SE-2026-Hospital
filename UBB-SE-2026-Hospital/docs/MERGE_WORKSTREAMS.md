# Merge Workstreams

This scaffold mirrors the technical reference and implementation plan.

## Folder expectations

- `Hospital.Shared/Models/StaffPharmacy`: 923-2 entities and cross-domain shared types
- `Hospital.Shared/Models/PatientEr`: 926-2 entities and ER models
- `Hospital.Shared/Repositories`: repository interfaces owned by the data layer
- `Hospital.Shared/Services`: service interfaces consumed by API/Web/Desktop
- `Hospital.Data/Repositories`: EF Core implementations
- `Hospital.Services/StaffPharmacy`: services ported from 923-2
- `Hospital.Services/PatientEr`: services ported from 926-2
- `Hospital.Desktop/Proxy`: HTTP proxies used by ViewModels

## Immediate next files to replace

- `Hospital.Shared/Models/*`: add the full merged entity set
- `Hospital.Data/HospitalDbContext.cs`: add all tables and relationships
- `Hospital.Services/DependencyInjection/ServiceCollectionExtensions.cs`: expand DI registration
- `Hospital.API/Controllers/*`: port all controllers from both APIs
- `Hospital.Web/Controllers/*`: port and consolidate MVC controllers

## Cross-team contracts already seeded here

- `IAuthService`
- `IAdminService`
- `IPatientService`
- `IUsersRepository`
- `IPatientRepository`

Add more interfaces here before large code moves so teams can work in parallel.
