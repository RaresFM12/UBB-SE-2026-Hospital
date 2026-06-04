# Manual Test Case: Billing - Apply and Persist Discount

Title: Apply discount to a medical record and persist
Preconditions: Application running, medical record ID `MR-1000` exists
Test Data: Medical Record ID `MR-1000`, Discount amount: `50.00`, Percentage: `25`
Steps:
  1. Log in as billing staff.
  2. Locate medical record `MR-1000` and apply 25% discount.
  3. Save changes and verify the persisted discount and final amount.
Expected Result: Discount applied correctly; persisted records reflect discount and total due is updated accordingly.
Postconditions / Cleanup: Revert changes if running against shared environment.
Priority: High
Owner: QA
Notes: Verify authorization and that discounts cannot be applied by unauthorized roles.