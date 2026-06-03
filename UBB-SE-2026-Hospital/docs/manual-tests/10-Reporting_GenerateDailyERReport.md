# Manual Test Case: Reporting - Generate Daily ER Report

Title: Generate daily ER visit report and verify contents
Preconditions: Application running, sample ER visits exist for the day
Test Data: Date: `YYYY-MM-DD` (today)
Steps:
  1. Log in as reporting user.
  2. Navigate to reporting and select ER daily report for the target date.
  3. Generate the report and export to PDF/CSV.
Expected Result: Report is generated with correct counts (visits, triage levels) and includes visit details. Export produces downloadable file.
Postconditions / Cleanup: N/A
Priority: Low
Owner: QA
Notes: Verify time zone handling for report date range.