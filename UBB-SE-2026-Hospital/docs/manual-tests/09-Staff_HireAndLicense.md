# Manual Test Case: Staff - Hire and License Assignment

Title: Create staff account and assign license number
Preconditions: Application running, admin user available
Test Data: Staff first name `John`, last name `Doe`, license `LIC-12345`
Steps:
  1. Log in as admin.
  2. Create new staff member with provided details.
  3. Assign license `LIC-12345` to the staff and save.
Expected Result: Staff record is created with `LicenseNumber` set. Staff can be searched and the license is displayed.
Postconditions / Cleanup: Remove test staff if required.
Priority: Medium
Owner: QA
Notes: Verify default license number behavior for empty input.