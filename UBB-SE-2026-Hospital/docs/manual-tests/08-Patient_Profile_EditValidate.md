# Manual Test Case: Patient - Edit and Validate Profile

Title: Edit patient profile and validate required fields
Preconditions: Application running, patient exists with ID `P1000`
Test Data: New phone number: `0712345678`, Invalid CNP: `abc`
Steps:
  1. Log in as staff with permission to edit patient data.
  2. Open patient `P1000` profile and change phone number to `0712345678`.
  3. Attempt to set CNP to `abc` and save.
Expected Result: Valid phone number is saved. Saving invalid CNP fails validation and displays an error. Patient profile remains unchanged for invalid fields.
Postconditions / Cleanup: Revert any valid changes made for test.
Priority: High
Owner: QA
Notes: Confirm validation messages and server-side validation if applicable.