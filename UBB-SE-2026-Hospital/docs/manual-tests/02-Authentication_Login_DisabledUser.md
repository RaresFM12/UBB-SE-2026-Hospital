# Manual Test Case: Authentication - Login Denied for Disabled User

Title: Login attempt with a disabled user account
Preconditions: Application running, user `disabled.user@hospital.local` exists and is marked disabled
Test Data: email: `disabled.user@hospital.local`, password: `Password123!`
Steps:
  1. Navigate to the login page.
  2. Enter the disabled user's credentials.
  3. Click `Login`.
Expected Result: Login is rejected and an appropriate error message is displayed. No authentication token/cookie is issued.
Postconditions / Cleanup: None
Priority: High
Owner: QA
Notes: Match error message text with localization if applicable.