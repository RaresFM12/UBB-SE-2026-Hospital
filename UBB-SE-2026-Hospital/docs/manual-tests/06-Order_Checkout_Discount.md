# Manual Test Case: Order - Checkout with Discount

Title: Place an order with an item discount and confirm totals
Preconditions: Application running, user with shopping cart exists, item `I-700` available
Test Data: Item `I-700`, Quantity `2`, Item discount `10%`, User discount `5%` (if applicable)
Steps:
  1. Add item `I-700` (quantity 2) to user's basket.
  2. Apply item-level discount and user-level discount if applicable.
  3. Proceed to checkout and place the order.
Expected Result: Order total reflects discounts correctly. Order items and quantities are recorded and inventory is decremented.
Postconditions / Cleanup: Cancel order or restore inventory if needed.
Priority: Medium
Owner: QA
Notes: Verify discount stacking rules and rounding precision.