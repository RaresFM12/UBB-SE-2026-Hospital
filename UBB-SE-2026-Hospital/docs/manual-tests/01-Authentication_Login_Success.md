# Manual Test Case: Authentication - Login Success

Title: Login with valid credentials
Preconditions: Application running, test user exists with email `test.user@hospital.local` and password `Password123!`
Test Data: email: `test.user@hospital.local`, password: `Password123!`
Steps:
  1. Navigate to the application login page.
  2. Enter the test user email and password.
  3. Click `Login`.
Expected Result: User is authenticated and redirected to the dashboard. A valid authentication cookie/token is issued.
Postconditions / Cleanup: Log out the user.
Priority: High
Owner: QA
Notes: Ensure the test user is not disabled and has a patient record if role-restricted flows are present.