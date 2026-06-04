# Manual Test Case: Inventory - Add and Remove Batch

Title: Add a new batch to an item and remove quantity
Preconditions: Application running, inventory item exists with Item ID `I-500`
Test Data: Item ID: `I-500`, Batch expiry: `YYYY-MM-DD` (future), Add quantity: `10`, Remove quantity: `3`
Steps:
  1. Log in as inventory manager.
  2. Add a new batch for `I-500` with the specified expiry and 10 packs.
  3. Confirm the batch appears in the item batches with correct quantity.
  4. Remove 3 units from the item (simulate dispensing).
Expected Result: The batch count reduces by 3 and total item quantity reflects the change.
Postconditions / Cleanup: Remove test batch if environment requires.
Priority: Medium
Owner: QA
Notes: Verify that batch selection logic uses the earliest expiry first for removal.