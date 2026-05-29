# UBB-SE-2026-Hospital

Starter solution for merging `923-2` and `926-2` into one hospital platform.

## Solution layout

- `Hospital.Shared`: shared entities, DTOs, interfaces, enums
- `Hospital.Data`: EF Core context and repository implementations
- `Hospital.Services`: business logic and DI registration
- `Hospital.API`: unified REST API
- `Hospital.Web`: unified MVC frontend
- `Hospital.Desktop`: unified WinUI 3 desktop shell with HTTP proxies
- `Hospital.Tests`: starter tests

## Intended merge flow

1. Move shared models and contracts into `Hospital.Shared`.
2. Merge both EF Core models into `Hospital.Data/HospitalDbContext`.
3. Move business logic into `Hospital.Services`.
4. Point `Hospital.API` and `Hospital.Web` only at services, not repositories.
5. Keep `Hospital.Desktop` talking to the API through `Proxy/`.

## Source repositories

- `../UBB-SE-2026-923-2-main`
- `../UBB-SE-2026-926-2-main`

## First workstreams

- Data team: finish the unified `HospitalDbContext` and repository contracts
- Services/API team: replace placeholder services/controllers with real implementations
- Web/Desktop team: port screens and navigation into the scaffolded projects
