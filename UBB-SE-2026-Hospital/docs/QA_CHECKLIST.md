# QA Checklist — Hospital Project

Purpose: quick checklist for manual and exploratory QA sessions covering critical flows, environment, and test data.

Scope: core Razor Pages flows (authentication, patient ER flows, prescriptions, orders, inventory, notifications) and service-level acceptance criteria.

Test environment
- Target: .NET 9 (net9.0)
- Browser(s): Chrome (latest), Edge (latest)
- API: run `Hospital.API` locally (launch profile) or use local test host
- Database: use local development database or in-memory/dev seed (use a dedicated test DB)
- Test account(s):
  - Admin: admin@example.test / seeded password
  - Regular user: user@example.test / seeded password

Test data notes
- Use non-production data; keep patient CNPs, emails, phone numbers synthetic
- Seeded entities: at least one patient, one ER visit, one examination, several items and batches, one order
- Reset DB state between manual test runs when possible

Pre-checks before testing
- Build solution: `dotnet build` — should succeed
- Run unit tests: `dotnet test Hospital.Tests` — no unexpected failures
- Start required services: `Hospital.API` and `Hospital.Web` local profiles

Critical flows (smoke/primary)
- Authentication: login with valid/invalid credentials, disabled user rejection
- Patient ER: create ER visit, triage, assign room, create examination, view patient history
- Prescriptions: view latest, filter, prescription detail
- Orders (Pharmacy): add items to basket, apply discounts, place order, view order status
- Inventory: add batch, change packs, remove batch, compute quantities at date
- Notifications: create, read/unread toggle, action button behavior

Checks to run for each flow
- Happy path: complete end-to-end scenario
- Validation: required fields missing, invalid values (e.g., CNP format, chief complaint length)
- Edge cases: duplicated entries, empty lists, expired items
- Permissions: admin-only tasks blocked for normal user
- Error handling: meaningful error messages, no unhandled exceptions

Acceptance criteria (high level)
- Feature passes all automated unit tests related to it
- No critical defects (blocking) in smoke flows
- UI displays expected data and formats (dates, names)

Reporting defects
- Include steps to reproduce, expected vs actual, environment, logs/screenshots, and a suggested severity
- Link failing unit tests (if any) and attach stack traces

Regression and release checklist
- Re-run unit and integration tests
- Run QA checklist smoke flows end-to-end
- Verify migrations/seeders applied and DB in expected state

Maintenance
- Keep this checklist in `docs/QA_CHECKLIST.md`
- Add new items as features are added (assign owner in task tracker)

Quick commands
- Build: `dotnet build`
- Tests: `dotnet test Hospital.Tests`
- Run web: open `Hospital.Web` startup profile in Visual Studio or `dotnet run --project Hospital.Web` in dev

Contact
- QA lead: @qa-lead (replace with actual user)

```
Placeholders: replace seeded credentials and environment details with your project's actual values before test runs.
```
