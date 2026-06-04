# Manual Test Case: Notification - Create and Read

Title: Create a notification for staff and mark it read
Preconditions: Application running, staff user exists with ID `S100`
Test Data: Title `System Maintenance`, Message `Scheduled maintenance at 02:00 UTC`
Steps:
  1. Create a notification targeting staff `S100` with the test title and message.
  2. Log in as `S100` and navigate to notifications.
  3. Open the notification and mark it as read.
Expected Result: Notification appears in user's notification list. After marking read, `IsRead` flag is true and notification is visually indicated as read.
Postconditions / Cleanup: Remove test notification if required.
Priority: Low
Owner: QA
Notes: Check that action buttons (if present) match `ActionButtonText`.