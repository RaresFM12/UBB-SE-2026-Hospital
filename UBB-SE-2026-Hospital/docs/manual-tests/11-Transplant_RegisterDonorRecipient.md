# Manual Test Case: Transplant - Register Donor and Recipient

Title: Register donor and recipient and verify compatibility check
Preconditions: Application running, donor and recipient candidates available
Test Data: Donor BloodType `O+`, Recipient BloodType `A+`
Steps:
  1. Log in as transplant coordinator.
  2. Create donor and recipient records with provided blood types.
  3. Run compatibility check.
Expected Result: Compatibility check returns expected result (e.g., O+ is compatible with A+). Records are linked if compatible.
Postconditions / Cleanup: Remove test records if necessary.
Priority: High
Owner: QA
Notes: Include Rh factor logic and crossmatch steps if present.