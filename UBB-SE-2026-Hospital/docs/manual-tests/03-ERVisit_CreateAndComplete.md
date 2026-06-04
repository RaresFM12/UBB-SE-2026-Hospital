# Manual Test Case: ER Visit - Create and Complete Flow

Title: Create an ER visit and complete triage and discharge
Preconditions: Application running, patient exists with ID `P12345`, staff user with ER role is available
Test Data: Patient ID: `P12345`, Chief complaint: `Severe abdominal pain`
Steps:
  1. Log in as ER staff.
  2. Create new ER visit for patient `P12345` with the chief complaint.
  3. Assign triage level and set initial vitals.
  4. Perform examination and record findings.
  5. Mark visit as discharged with final diagnosis.
Expected Result: The ER visit is created, triage and examination entries are recorded, and the visit status changes to discharged. All timestamps are recorded.
Postconditions / Cleanup: Restore any test data if created in production-like environment.
Priority: High
Owner: QA
Notes: Verify audit trail entries for staff actions.