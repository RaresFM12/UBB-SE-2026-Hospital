# Manual Test Case: Prescription - Create and Dispense

Title: Create a prescription and dispense medication
Preconditions: Application running, patient exists, pharmacy staff user available, medication item in inventory with sufficient quantity
Test Data: Patient: `P12345`, Medication Item ID: `MED-100`, Quantity: `2`
Steps:
  1. Log in as prescribing staff.
  2. Create a prescription for patient `P12345` with 2 units of `MED-100`.
  3. Save prescription and verify it appears in the pharmacy queue.
  4. Log in as pharmacy staff and dispense the prescription.
Expected Result: Prescription is created, visible in pharmacy queue, and dispensing reduces inventory by the dispensed quantity. Prescription status updated (e.g., to dispensed).
Postconditions / Cleanup: Revert inventory changes if running against shared environment.
Priority: High
Owner: QA
Notes: Check for alerts on controlled substances or addiction flags.