using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Reference data (HasData) ──────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Allergies",
                columns: new[] { "AllergyId", "AllergyCategory", "AllergyName", "AllergyType" },
                values: new object[,]
                {
                    { 1,  "Antibiotic",   "Penicillin",   "Drug"          },
                    { 2,  "Nut",          "Peanuts",       "Food"          },
                    { 3,  "Material",     "Latex",         "Contact"       },
                    { 4,  "NSAID",        "Ibuprofen",     "Drug"          },
                    { 5,  "Antibiotic",   "Sulfonamides",  "Drug"          },
                    { 6,  "Seafood",      "Shellfish",     "Food"          },
                    { 7,  "Seasonal",     "Pollen",        "Environmental" },
                    { 8,  "Perennial",    "Dust Mites",    "Environmental" },
                    { 9,  "Salicylate",   "Aspirin",       "Drug"          },
                    { 10, "Lactose",      "Dairy",         "Food"          }
                });

            migrationBuilder.InsertData(
                table: "HighRiskMedicines",
                columns: new[] { "Id", "MedicineName", "WarningMessage" },
                values: new object[,]
                {
                    { 1, "Warfarin",     "Anticoagulant - check INR before prescribing."         },
                    { 2, "Methotrexate", "Hepatotoxic - confirm dosing and weekly schedule."     }
                });

            migrationBuilder.InsertData(
                table: "Substances",
                columns: new[] { "Id", "Description", "LethalDose", "Name" },
                values: new object[,]
                {
                    {  1, "Anti-inflammatory pain reliever",               3200f, "Ibuprofen"       },
                    {  2, "Pain reliever and fever reducer",                4000f, "Paracetamol"     },
                    {  3, "Mineral supplement for muscle and nerve support",2500f, "Magnesium"       },
                    {  4, "Vitamin supplement for immune support",          2000f, "Vitamin C"       },
                    {  5, "Antihistamine for allergy relief",                500f, "Cetirizine"      },
                    {  6, "Mineral supplement used for iron deficiency",      45f, "Iron"            },
                    {  7, "Mineral supplement for bones and muscles",       2500f, "Calcium"         },
                    {  8, "Fatty acid supplement for heart and brain health",3000f,"Omega 3"         },
                    {  9, "Sleep support supplement",                         10f, "Melatonin"       },
                    { 10, "Digestive support supplement",                   1000f, "Probiotics"      },
                    { 11, "Mineral supplement for immunity",                  40f, "Zinc"            },
                    { 12, "Non-drowsy antihistamine",                       1000f, "Loratadine"      },
                    { 13, "Medication to decrease frequency of diarrhea",     60f, "Loperamide"      },
                    { 14, "Anti-foaming agent to reduce bloating and gas",  2000f, "Simethicone"     },
                    { 15, "Nonsteroidal anti-inflammatory drug (NSAID)",    1500f, "Diclofenac"      },
                    { 16, "Skin protectant and moisturizer",                5000f, "Dexpanthenol"    },
                    { 17, "Essential vitamin for bone health and immunity",    50f, "Vitamin D3"     },
                    { 18, "Decongestant for nasal passages",                  10f, "Xylometazoline"  },
                    { 19, "Mucolytic agent to clear mucus",                 3000f, "Acetylcysteine"  }
                });

            // ── 2. Users ──────────────────────────────────────────────────────────
            // Using plain / demo password hashes matching source project pattern
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PhoneNumber", "PasswordHash", "IsDisabled", "IsAdmin", "Username", "Role",
                                 "DiscountNotifications", "LoyaltyPoints", "StartPeriodDate", "CycleDays", "PeriodLasts", "PremenstrualSyndromeOption" },
                values: new object[,]
                {
                    { 1, "admin@pharmacy.local", "0700000000", "hashed_pwd_admin", false, true,  "admin_super",  "Admin",      true,  1000, new DateOnly(1900,1,1), 28, 5, 0 },
                    { 2, "johndoe@test.com",     "0711111111", "hashed_pwd_john",  false, false, "johndoe",      "Client",     true,   150, new DateOnly(1900,1,1), 28, 5, 0 },
                    { 3, "janedoe@test.com",     "0722222222", "hashed_pwd_jane",  false, false, "janedoe",      "Client",     false,   45, new DateOnly(2026,1,1), 28, 5, 0 },
                    { 4, "house@hospital.local", "0733333333", "hashed_pwd_house", false, false, "dr_house",     "Doctor",     false,    0, new DateOnly(1900,1,1), 28, 5, 0 },
                    { 5, "jamie@hospital.local", "0744444444", "hashed_pwd_jamie", false, false, "jamie_pharm",  "Pharmacist", false,    0, new DateOnly(1900,1,1), 28, 5, 0 },
                    { 6, "paul@gmail.com",       "0744444444", "abc123",           false, true,  "paul",         "Admin",      false,    0, new DateOnly(1900,1,1), 28, 5, 0 },
                    { 7, "paull@gmail.com",      "0744444445", "abc123",           false, false, "paull",        "Client",     false,    0, new DateOnly(1900,1,1), 28, 5, 0 }
                });

            // ── 3. Staff (TPH: Staff / Doctor / Pharmacist) ───────────────────────
            // Use raw SQL to bypass EF column name mapping issue with StaffID PK
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Staff] ON;
                INSERT INTO [Staff] ([StaffID],[Email],[PasswordHash],[Role],[Department],[FirstName],[LastName],[ContactInfo],[Available],[LicenseNumber],[Specialization],[Status],[Certification],[YearsOfExperience],[HourlyRate],[DoctorStatus]) VALUES
                (1,'house@hospital.local', 'hash','Doctor',    'Diagnostics','Gregory','House', '555-0101',1,'LIC-1001','Diagnostician','Available','Board Certified',10,150.0,0),
                (2,'wilson@hospital.local','hash','Doctor',    'Oncology',   'James',  'Wilson','555-0102',1,'LIC-1002','Diagnostician','Available','Board Certified', 8,140.0,0),
                (3,'cuddy@hospital.local', 'hash','Doctor',    'Admin',      'Lisa',   'Cuddy', '555-0103',1,'LIC-1003','Surgery',      'Available','Board Certified',12,160.0,0),
                (4,'jamie@hospital.local', 'hash','Pharmacist','Pharmacy',   'Jamie',  'Chen',  '555-0104',1,'LIC-1004','Nurse',        'Available','Compounding',     4, 80.0,NULL),
                (5,'pat@hospital.local',   'hash','Pharmacist','Pharmacy',   'Pat',    'Moore', '555-0105',1,'LIC-1005','Doctor',       'Available','Hospital',        6, 85.0,NULL);
                SET IDENTITY_INSERT [Staff] OFF;
            ");

            // ── 4. Patients ───────────────────────────────────────────────────────
            // Sex enum: 0 = Male, 1 = Female  (from Sex.cs)
            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "FirstName", "LastName", "Cnp", "DateOfBirth", "Sex",
                                 "PhoneNumber", "EmergencyContact", "IsArchived", "IsDonor", "Transferred" },
                values: new object[,]
                {
                    {  1, "Alice",   "Johnson",   "1900101123456", new DateTime(1990,1,1),  1, "0700000001", "Bob Johnson 0700000002",      false, false, false },
                    {  2, "Bob",     "Smith",     "1850215234567", new DateTime(1985,2,15), 0, "0700000003", "Carol Smith 0700000004",      false, true,  false },
                    {  3, "Carol",   "Williams",  "1921103345678", new DateTime(1992,11,3), 1, "0700000005", "David Williams 0700000006",   false, false, false },
                    {  4, "David",   "Brown",     "1780420456789", new DateTime(1978,4,20), 0, "0700000007", "Eve Brown 0700000008",        false, false, false },
                    {  5, "Eve",     "Jones",     "1950607567890", new DateTime(1995,6,7),  1, "0700000009", "Frank Jones 0700000010",      false, true,  false },
                    {  6, "Frank",   "Garcia",    "1680812678901", new DateTime(1968,8,12), 0, "0700000011", "Grace Garcia 0700000012",     false, false, false },
                    {  7, "Grace",   "Martinez",  "2001009789012", new DateTime(2000,10,9), 1, "0700000013", "Henry Martinez 0700000014",   false, false, false },
                    {  8, "Henry",   "Davis",     "1731215890123", new DateTime(1973,12,15),0, "0700000015", "Irene Davis 0700000016",      false, true,  false },
                    {  9, "Irene",   "Rodriguez", "1880322901234", new DateTime(1988,3,22), 1, "0700000017", "Jack Rodriguez 0700000018",   false, false, false },
                    { 10, "Jack",    "Wilson",    "1820530012345", new DateTime(1982,5,30), 0, "0700000019", "Karen Wilson 0700000020",     false, false, false },
                    { 11, "Karen",   "Anderson",  "1970718123456", new DateTime(1997,7,18), 1, "0700000021", "Leo Anderson 0700000022",     false, false, false },
                    { 12, "Leo",     "Taylor",    "1760924234567", new DateTime(1976,9,24), 0, "0700000023", "Mia Taylor 0700000024",       false, true,  false },
                    { 13, "Mia",     "Thomas",    "2020105345678", new DateTime(2002,1,5),  1, "0700000025", "Noah Thomas 0700000026",      false, false, false },
                    { 14, "Noah",    "Hernandez", "1870311456789", new DateTime(1987,3,11), 0, "0700000027", "Olivia Hernandez 0700000028", false, false, false },
                    { 15, "Olivia",  "Moore",     "1930417567890", new DateTime(1993,4,17), 1, "0700000029", "Peter Moore 0700000030",      false, true,  false },
                    { 16, "Peter",   "Jackson",   "1710623678901", new DateTime(1971,6,23), 0, "0700000031", "Quinn Jackson 0700000032",    false, false, false },
                    { 17, "Quinn",   "Martin",    "1990801789012", new DateTime(1999,8,1),  1, "0700000033", "Ryan Martin 0700000034",      false, false, false },
                    { 18, "Ryan",    "Lee",       "1801008890123", new DateTime(1980,10,8), 0, "0700000035", "Sarah Lee 0700000036",        false, false, false },
                    { 19, "Sarah",   "Perez",     "1961214901234", new DateTime(1996,12,14),1, "0700000037", "Tom Perez 0700000038",        false, true,  false },
                    { 20, "Tom",     "Thompson",  "1830220012345", new DateTime(1983,2,20), 0, "0700000039", "Uma Thompson 0700000040",     false, false, false }
                });

            // ── 5. MedicalHistories ───────────────────────────────────────────────
            // BloodType enum: 0=A, 1=B, 2=AB, 3=O  / Rh enum: 0=Positive, 1=Negative
            migrationBuilder.InsertData(
                table: "MedicalHistories",
                columns: new[] { "MedicalHistoryId", "PatientId", "BloodType", "Rh", "ChronicConditions" },
                values: new object[,]
                {
                    {  1,  1, 0, 0, "[]" },
                    {  2,  2, 3, 0, "[\"Hypertension\"]" },
                    {  3,  3, 1, 1, "[]" },
                    {  4,  4, 2, 0, "[\"Diabetes Type 2\"]" },
                    {  5,  5, 0, 1, "[]" },
                    {  6,  6, 1, 0, "[\"Asthma\"]" },
                    {  7,  7, 3, 1, "[]" },
                    {  8,  8, 0, 0, "[\"Chronic Back Pain\"]" },
                    {  9,  9, 2, 1, "[]" },
                    { 10, 10, 1, 0, "[\"Hyperlipidemia\"]" },
                    { 11, 11, 3, 0, "[]" },
                    { 12, 12, 0, 1, "[\"COPD\"]" },
                    { 13, 13, 1, 0, "[]" },
                    { 14, 14, 2, 0, "[\"Hypertension\",\"Diabetes Type 2\"]" },
                    { 15, 15, 3, 1, "[]" },
                    { 16, 16, 0, 0, "[\"Arthritis\"]" },
                    { 17, 17, 1, 1, "[]" },
                    { 18, 18, 2, 0, "[\"Hypothyroidism\"]" },
                    { 19, 19, 3, 0, "[]" },
                    { 20, 20, 0, 1, "[\"Heart Disease\"]" }
                });

            // ── 6. PatientAllergies ───────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "PatientAllergies",
                columns: new[] { "MedicalHistoryId", "AllergyId", "SeverityLevel" },
                values: new object[,]
                {
                    {  1, 1, "Severe"   },
                    {  2, 2, "Moderate" },
                    {  3, 3, "Mild"     },
                    {  4, 4, "Moderate" },
                    {  5, 5, "Severe"   },
                    {  6, 7, "Mild"     },
                    {  7, 8, "Moderate" },
                    {  8, 9, "Mild"     },
                    {  9, 1, "Moderate" },
                    { 10, 6, "Mild"     }
                });

            // ── 7. MedicalRecords ─────────────────────────────────────────────────
            // SourceType enum: stored as int (0=ERVisit, 1=Prescription... check model)
            migrationBuilder.InsertData(
                table: "MedicalRecords",
                columns: new[] { "RecordId", "MedicalHistoryId", "SourceType", "SourceId", "StaffId",
                                 "Symptoms", "Diagnosis", "ConsultationDate",
                                 "BasePrice", "FinalPrice", "DiscountApplied", "PoliceNotified", "TransplantId" },
                values: new object[,]
                {
                    { 1,  1, 0, 1, 1, "Headache, fever",        "Influenza",      new DateTime(2026,4,10), 150.00m, 150.00m, null, false, null },
                    { 2,  2, 0, 2, 2, "Joint pain",             "Arthritis",      new DateTime(2026,4,11), 200.00m, 200.00m, null, false, null },
                    { 3,  3, 0, 3, 3, "Nausea",                 "Gastritis",      new DateTime(2026,4,12), 120.00m, 120.00m, null, false, null },
                    { 4,  4, 0, 4, 1, "Chest pain",             "Angina",         new DateTime(2026,4,13), 300.00m, 300.00m, null, false, null },
                    { 5,  5, 0, 5, 2, "Skin rash",              "Allergy",        new DateTime(2026,4,14), 100.00m, 100.00m, null, false, null },
                    { 6,  6, 0, 6, 3, "Insomnia",               "Anxiety Disorder",new DateTime(2026,4,15),180.00m, 180.00m, null, false, null },
                    { 7,  7, 0, 7, 1, "Runny nose",             "Rhinitis",       new DateTime(2026,4,16), 90.00m,  90.00m,  null, false, null },
                    { 8,  8, 0, 8, 2, "Back pain",              "Lumbar strain",  new DateTime(2026,4,17), 220.00m, 220.00m, null, false, null },
                    { 9,  9, 0, 9, 3, "Diarrhea",               "Gastroenteritis",new DateTime(2026,4,18), 130.00m, 130.00m, null, false, null },
                    { 10,10, 0,10, 1, "Cough",                  "Bronchitis",     new DateTime(2026,4,19), 160.00m, 160.00m, null, false, null }
                });

            // ── 8. Prescriptions ──────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Prescriptions",
                columns: new[] { "PrescriptionId", "RecordId", "DoctorNotes", "Date" },
                values: new object[,]
                {
                    {  1,  1, "Take with food",              new DateTime(2026,4,10) },
                    {  2,  2, "Physiotherapy recommended",   new DateTime(2026,4,11) },
                    {  3,  3, "Follow-up in 1 week",         new DateTime(2026,4,12) },
                    {  4,  4, "Cardiology referral",         new DateTime(2026,4,13) },
                    {  5,  5, "Allergy testing recommended", new DateTime(2026,4,14) }
                });

            // ── 9. PrescriptionItems ──────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "PrescriptionItems",
                columns: new[] { "PrescriptionItemId", "PrescriptionId", "MedicationName", "Quantity" },
                values: new object[,]
                {
                    {  1, 1, "Panadol Extra",      "2x daily"  },
                    {  2, 1, "Nurofen Express",    "1x daily"  },
                    {  3, 2, "Voltaren Gel",       "3x daily"  },
                    {  4, 3, "Espumisan",          "2x daily"  },
                    {  5, 4, "Aspirin",            "1x daily"  },
                    {  6, 5, "Zyrtec",             "1x daily"  }
                });

            // ── 10. ERRooms ───────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "ERRooms",
                columns: new[] { "RoomId", "RoomTypeName", "AvailabilityStatus", "CurrentVisitId" },
                values: new object[,]
                {
                    {  1, "General Examination Room",          "Available", null },
                    {  2, "General Examination Room",          "Available", null },
                    {  3, "General Examination Room",          "Available", null },
                    {  4, "General Examination Room",          "Available", null },
                    {  5, "Trauma/Resuscitation Bay",          "Available", null },
                    {  6, "Trauma/Resuscitation Bay",          "Available", null },
                    {  7, "Respiratory/Monitored Room",        "Available", null },
                    {  8, "Respiratory/Monitored Room",        "Available", null },
                    {  9, "Neurology/Quiet Observation Room",  "Available", null },
                    { 10, "Neurology/Quiet Observation Room",  "Available", null },
                    { 11, "Orthopedic/Procedure Room",         "Available", null },
                    { 12, "Orthopedic/Procedure Room",         "Available", null },
                    { 13, "Operating Room (OR)",               "Available", null },
                    { 14, "Operating Room (OR)",               "Available", null },
                    { 15, "General Examination Room",          "Available", null },
                    { 16, "General Examination Room",          "Available", null },
                    { 17, "Trauma/Resuscitation Bay",          "Available", null },
                    { 18, "Respiratory/Monitored Room",        "Available", null },
                    { 19, "Neurology/Quiet Observation Room",  "Available", null },
                    { 20, "Orthopedic/Procedure Room",         "Available", null }
                });

            // ── 11. ERVisits ──────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "ERVisits",
                columns: new[] { "VisitId", "PatientId", "ArrivalDateTime", "ChiefComplaint", "Status" },
                values: new object[,]
                {
                    {  1,  1, new DateTime(2026,4,1,8,0,0),  "Chest pain",              "CLOSED" },
                    {  2,  2, new DateTime(2026,4,1,9,0,0),  "Shortness of breath",     "CLOSED" },
                    {  3,  3, new DateTime(2026,4,1,10,0,0), "Severe headache",         "CLOSED" },
                    {  4,  4, new DateTime(2026,4,2,8,0,0),  "Abdominal pain",          "CLOSED" },
                    {  5,  5, new DateTime(2026,4,2,9,0,0),  "Dizziness",               "CLOSED" },
                    {  6,  6, new DateTime(2026,4,2,10,0,0), "Fever 39C",               "CLOSED" },
                    {  7,  7, new DateTime(2026,4,3,8,0,0),  "Fall injury",             "CLOSED" },
                    {  8,  8, new DateTime(2026,4,3,9,0,0),  "Back pain",               "CLOSED" },
                    {  9,  9, new DateTime(2026,4,3,10,0,0), "Vomiting",                "CLOSED" },
                    { 10, 10, new DateTime(2026,4,4,8,0,0),  "Allergic reaction",       "CLOSED" },
                    { 11, 11, new DateTime(2026,4,4,9,0,0),  "Palpitations",            "CLOSED" },
                    { 12, 12, new DateTime(2026,4,4,10,0,0), "Difficulty breathing",    "CLOSED" },
                    { 13, 13, new DateTime(2026,4,5,8,0,0),  "Nausea",                  "CLOSED" },
                    { 14, 14, new DateTime(2026,4,5,9,0,0),  "Hypertensive crisis",     "CLOSED" },
                    { 15, 15, new DateTime(2026,4,5,10,0,0), "Broken wrist",            "CLOSED" },
                    { 16, 16, new DateTime(2026,4,6,8,0,0),  "Chest pain",              "REGISTERED" },
                    { 17, 17, new DateTime(2026,4,6,9,0,0),  "Severe migraine",         "TRIAGED" },
                    { 18, 18, new DateTime(2026,4,6,10,0,0), "Abdominal cramps",        "TRIAGED" },
                    { 19, 19, new DateTime(2026,4,7,8,0,0),  "Ankle injury",            "REGISTERED" },
                    { 20, 20, new DateTime(2026,4,7,9,0,0),  "Shortness of breath",     "REGISTERED" }
                });

            // ── 12. Triages ───────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Triages",
                columns: new[] { "TriageId", "VisitId", "TriageLevel", "Specialization", "NurseId", "TriageTime" },
                values: new object[,]
                {
                    {  1,  1, 1, "Cardiology",  1, new DateTime(2026,4,1,8,10,0)  },
                    {  2,  2, 2, "Pulmonology", 2, new DateTime(2026,4,1,9,10,0)  },
                    {  3,  3, 3, "Neurology",   3, new DateTime(2026,4,1,10,10,0) },
                    {  4,  4, 3, "Surgery",     1, new DateTime(2026,4,2,8,10,0)  },
                    {  5,  5, 4, "Neurology",   2, new DateTime(2026,4,2,9,10,0)  },
                    {  6,  6, 4, "General",     3, new DateTime(2026,4,2,10,10,0) },
                    {  7,  7, 3, "Orthopedics", 1, new DateTime(2026,4,3,8,10,0)  },
                    {  8,  8, 4, "Orthopedics", 2, new DateTime(2026,4,3,9,10,0)  },
                    {  9,  9, 4, "General",     3, new DateTime(2026,4,3,10,10,0) },
                    { 10, 10, 2, "Allergology", 1, new DateTime(2026,4,4,8,10,0)  },
                    { 11, 11, 2, "Cardiology",  2, new DateTime(2026,4,4,9,10,0)  },
                    { 12, 12, 1, "Pulmonology", 3, new DateTime(2026,4,4,10,10,0) },
                    { 13, 13, 4, "General",     1, new DateTime(2026,4,5,8,10,0)  },
                    { 14, 14, 1, "Cardiology",  2, new DateTime(2026,4,5,9,10,0)  },
                    { 15, 15, 3, "Orthopedics", 3, new DateTime(2026,4,5,10,10,0) },
                    { 16, 16, 2, "Cardiology",  1, new DateTime(2026,4,6,8,10,0)  },
                    { 17, 17, 3, "Neurology",   2, new DateTime(2026,4,6,9,10,0)  },
                    { 18, 18, 4, "General",     3, new DateTime(2026,4,6,10,10,0) },
                    { 19, 19, 3, "Orthopedics", 1, new DateTime(2026,4,7,8,10,0)  },
                    { 20, 20, 2, "Pulmonology", 2, new DateTime(2026,4,7,9,10,0)  }
                });

            // ── 13. TriageParameters ──────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "TriageParameters",
                columns: new[] { "TriageParametersId", "TriageId", "Consciousness", "Breathing", "Bleeding", "InjuryType", "PainLevel" },
                values: new object[,]
                {
                    {  1,  1, 1, 1, 1, 1, 3 },
                    {  2,  2, 1, 2, 1, 1, 2 },
                    {  3,  3, 2, 1, 1, 2, 3 },
                    {  4,  4, 1, 1, 2, 2, 2 },
                    {  5,  5, 2, 1, 1, 1, 2 },
                    {  6,  6, 1, 1, 1, 1, 1 },
                    {  7,  7, 1, 1, 2, 3, 2 },
                    {  8,  8, 1, 1, 1, 2, 2 },
                    {  9,  9, 1, 1, 1, 1, 1 },
                    { 10, 10, 1, 1, 1, 1, 2 },
                    { 11, 11, 1, 2, 1, 1, 2 },
                    { 12, 12, 1, 3, 1, 1, 2 },
                    { 13, 13, 1, 1, 1, 1, 1 },
                    { 14, 14, 1, 1, 1, 1, 3 },
                    { 15, 15, 1, 1, 2, 3, 2 },
                    { 16, 16, 1, 2, 1, 1, 2 },
                    { 17, 17, 2, 1, 1, 2, 3 },
                    { 18, 18, 1, 1, 1, 1, 2 },
                    { 19, 19, 1, 1, 2, 3, 2 },
                    { 20, 20, 1, 2, 1, 1, 2 }
                });

            // ── 14. Examinations ──────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Examinations",
                columns: new[] { "ExaminationId", "VisitId", "DoctorId", "RoomId", "ExaminationDate", "Findings", "Recommendation" },
                values: new object[,]
                {
                    {  1,  1, 1,  1, new DateTime(2026,4,1,9,0,0),  "ECG normal, stress-related pain", "Rest and follow-up in 1 week" },
                    {  2,  2, 2,  2, new DateTime(2026,4,1,10,0,0), "Mild asthma exacerbation",        "Inhaler prescribed, follow-up" },
                    {  3,  3, 3,  3, new DateTime(2026,4,1,11,0,0), "Tension headache",                "Analgesics and rest" },
                    {  4,  4, 1,  4, new DateTime(2026,4,2,9,0,0),  "Acute gastritis",                 "Antacids and diet change" },
                    {  5,  5, 2,  5, new DateTime(2026,4,2,10,0,0), "Benign positional vertigo",       "Vestibular exercises" },
                    {  6,  6, 3,  6, new DateTime(2026,4,2,11,0,0), "Viral infection",                 "Antipyretics and rest" },
                    {  7,  7, 1,  7, new DateTime(2026,4,3,9,0,0),  "Soft tissue injury to elbow",     "Ice and elevation" },
                    {  8,  8, 2,  8, new DateTime(2026,4,3,10,0,0), "Lumbar muscle strain",            "Physiotherapy referral" },
                    {  9,  9, 3,  9, new DateTime(2026,4,3,11,0,0), "Viral gastroenteritis",           "Hydration and rest" },
                    { 10, 10, 1, 10, new DateTime(2026,4,4,9,0,0),  "Urticaria from food allergy",     "Antihistamines" }
                });

            // ── 15. TransferLogs ──────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "TransferLogs",
                columns: new[] { "TransferLogId", "VisitId", "TransferTime", "TargetSystem", "FilePath", "Status" },
                values: new object[,]
                {
                    { 1,  1, new DateTime(2026,4,1,12,0,0), "Internal",  null, "Completed" },
                    { 2,  2, new DateTime(2026,4,1,13,0,0), "External",  null, "Completed" },
                    { 3, 11, new DateTime(2026,4,4,12,0,0), "Internal",  null, "Completed" },
                    { 4, 12, new DateTime(2026,4,4,13,0,0), "External",  null, "Pending"   }
                });

            // ── 16. Items ─────────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Id", "Name", "Price", "Category", "NumberOfPills", "Producer",
                                 "ImagePath", "Quantity", "Label", "Description", "DiscountPercentage" },
                values: new object[,]
                {
                    {  1,  "Nurofen Express",           28.5f,  "pain relief",   20, "Reckitt",       "Assets/nurofen.png",        40, "Fast pain relief",      "Ibuprofen capsules for pain and inflammation",          0f },
                    {  2,  "Panadol Extra",             19.99f, "pain relief",   16, "GSK",           "Assets/panadol.png",         0, "Extra strength",        "Paracetamol tablets for headaches and fever",          10f },
                    {  3,  "Magne B6",                  32f,    "wellness",      50, "Sanofi",        "Assets/magneb6.png",         25, "Magnesium support",    "Magnesium and vitamin B6 supplement",                   0f },
                    {  4,  "Feroglobin",                36.5f,  "wellness",      30, "Vitabiotics",   "Assets/feroglobin.png",      18, "Iron formula",         "Iron supplement for energy and blood health",           5f },
                    {  5,  "Vitamin C 1000",            22f,    "wellness",      20, "NaturPharma",   "Assets/vitaminc.png",        50, "Immune support",       "High strength vitamin C tablets",                       0f },
                    {  6,  "Calcium + D3",              27.5f,  "wellness",      30, "BioFarm",       "Assets/calciumd3.png",       22, "Bone support",         "Calcium and vitamin D3 supplement",                    15f },
                    {  7,  "Omega 3 Forte",             45f,    "wellness",      60, "Doppelherz",    "Assets/omega3.png",          14, "Heart support",        "Omega 3 capsules for heart and brain",                  0f },
                    {  8,  "Melatonin Sleep",           18f,    "wellness",      30, "Walmark",       "Assets/melatonin.png",       12, "Sleep support",        "Melatonin tablets for better sleep",                    0f },
                    {  9,  "Probiotic Balance",         39.99f, "wellness",      20, "Secom",         "Assets/probiotic.png",        0, "Digestive comfort",    "Daily probiotic capsules",                             20f },
                    { 10,  "Zinc Complex",              21.5f,  "wellness",      30, "NaturMil",      "Assets/zinc.png",            28, "Immune defense",       "Zinc supplement for immune support",                    0f },
                    { 11,  "Coldrex MaxGrip",           31f,    "cold and flu",  10, "GSK",           "Assets/coldrex.png",         20, "Cold relief",          "Powder for cold and flu symptoms",                      0f },
                    { 12,  "Strepsils Intensive",       24f,    "cold and flu",  24, "Reckitt",       "Assets/strepsils.png",       17, "Sore throat relief",   "Lozenges for sore throat",                              0f },
                    { 13,  "No-Spa Forte",              26f,    "pain relief",   24, "Sanofi",        "Assets/nospa.png",           30, "Cramp relief",         "Drotaverine tablets for cramps",                        0f },
                    { 14,  "Femina Comfort",            29.5f,  "wellness",      30, "HerbalLab",     "Assets/femina.png",           0, "Period wellness",      "Supplement designed for menstrual comfort",             10f },
                    { 15,  "Herbal Relax Tea Capsules", 23.5f,  "wellness",      20, "PlantMed",      "Assets/herbalrelax.png",     21, "Relax support",        "Natural calming capsules for stress relief",             0f },
                    { 16,  "Zyrtec",                    25.5f,  "allergy",       20, "UCB",           "Assets/zyrtec.png",          40, "24 Hour Relief",       "Cetirizine tablets for indoor and outdoor allergies",    0f },
                    { 17,  "Claritine",                 24f,    "allergy",       30, "Bayer",         "Assets/claritine.png",       35, "Non-Drowsy",           "Loratadine allergy relief tablets",                    10f },
                    { 18,  "Imodium",                   18.5f,  "digestion",     12, "J&J",           "Assets/imodium.png",         50, "Fast Acting",          "Loperamide capsules for diarrhea relief",               0f },
                    { 19,  "Espumisan",                 22f,    "digestion",     50, "Berlin-Chemie", "Assets/espumisan.png",       60, "Anti-Bloating",        "Simethicone capsules for gas relief",                   5f },
                    { 20,  "Colebil",                   15f,    "digestion",     20, "Biofarm",       "Assets/colebil.png",          0, "Bile Support",         "Digestive supplement after heavy meals",                 0f },
                    { 21,  "Smecta",                    19.5f,  "digestion",     10, "Ipsen",         "Assets/smecta.png",          30, "Digestive Protectant", "Powder for oral suspension",                            0f },
                    { 22,  "Voltaren Gel",              35f,    "pain relief",    1, "GSK",           "Assets/voltaren.png",         0, "Targeted Pain Relief", "Diclofenac topical gel for joint and muscle pain",     15f },
                    { 23,  "Bepanthen Ointment",        28f,    "skincare",       1, "Bayer",         "Assets/bepanthen.png",       40, "Skin Repair",          "Dexpanthenol ointment for skin irritation",             0f },
                    { 24,  "Sudocrem",                  26.5f,  "skincare",       1, "Teva",          "Assets/sudocrem.png",        55, "Healing Cream",        "Antiseptic healing cream for diaper rash and eczema",   0f },
                    { 25,  "Cerave Cleanser",           55f,    "skincare",       1, "L'Oreal",       "Assets/cerave.png",          20, "Hydrating Formula",   "Daily facial cleanser with ceramides",                  20f },
                    { 26,  "Centrum Men",               65f,    "wellness",      30, "GSK",           "Assets/centrum_men.png",     15, "Multivitamin",         "Complete daily multivitamin for men",                   0f },
                    { 27,  "Centrum Women",             65f,    "wellness",      30, "GSK",           "Assets/centrum_women.png",   15, "Multivitamin",         "Complete daily multivitamin for women",                 0f },
                    { 28,  "Supradyn Energy",           48f,    "wellness",      30, "Bayer",         "Assets/supradyn.png",        22, "Energy Support",       "Vitamins with CoQ10 for energy release",               10f },
                    { 29,  "Vitamin D3 2000 IU",        15.99f, "wellness",      60, "NaturPharma",   "Assets/vitamind3.png",       80, "Sun Vitamin",          "High-dose Vitamin D3 softgels",                         0f },
                    { 30,  "B-Complex Forte",           21f,    "wellness",      30, "Zentiva",       "Assets/bcomplex.png",        40, "Nerve Support",        "High strength B-vitamins",                              0f },
                    { 31,  "Betadine Solution",         18f,    "first aid",      1, "Egis",          "Assets/betadine.png",         0, "Antiseptic",           "Povidone-iodine topical solution for wound care",        0f },
                    { 32,  "Sterile Plasters",          12.5f,  "first aid",     50, "Urgo",          "Assets/plasters.png",       100, "Waterproof",           "Assorted sizes of waterproof bandages",                 0f },
                    { 33,  "Olynth Nasal Spray",        16.5f,  "cold and flu",   1, "J&J",           "Assets/olynth.png",           0, "Decongestant",         "Xylometazoline spray for unblocking the nose",          0f },
                    { 34,  "ACC 600",                   29f,    "cold and flu",  10, "Sandoz",        "Assets/acc600.png",          30, "Mucus Clearance",      "Effervescent tablets for productive coughs",            0f },
                    { 35,  "Theraflu Extra",            33f,    "cold and flu",  10, "GSK",           "Assets/theraflu.png",        25, "Severe Cold",          "Hot liquid powder for severe cold symptoms",           10f },
                    { 36,  "Paracetamol Generic",       9.99f,  "pain relief",   16, "Generic Pharma","Assets/paracetamol.png",     30, "Generic",              "Generic paracetamol tablets",                           0f },
                    { 100, "Ibuprofen (Generic)",       18.5f,  "Pain Relief",   20, "Terapia",       "Assets/placeholder.png",     0, "Anti-inflammatory",    "Used for pain and inflammation",                        0f },
                    { 101, "Aspirin",                   15f,    "Pain Relief",   30, "Bayer",         "Assets/placeholder.png",     0, "Painkiller",           "Used for headaches and fever",                          0f },
                    { 102, "Amoxicillin",               32f,    "Antibiotic",    16, "Sandoz",        "Assets/placeholder.png",     0, "Antibiotic",           "Broad-spectrum antibiotic",                             0f },
                    { 103, "Cetirizine (Generic)",      22f,    "Allergy",       10, "Zentiva",       "Assets/placeholder.png",     0, "Antihistamine",        "Used for allergies",                                    0f },
                    { 104, "Omeprazole",                27f,    "Digestive",     14, "Krka",          "Assets/placeholder.png",     0, "Stomach protection",   "Reduces stomach acid",                                  0f }
                });

            // ── 17. ItemBatches ───────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "ItemBatches",
                columns: new[] { "Id", "ItemId", "ExpirationDate", "NumberOfPacks" },
                values: new object[,]
                {
                    {  1,  1, new DateOnly(2027,1,1),  40 },
                    {  2,  2, new DateOnly(2027,1,1),  35 },
                    {  3,  3, new DateOnly(2027,1,1),  25 },
                    {  4,  4, new DateOnly(2027,1,1),  18 },
                    {  5,  5, new DateOnly(2027,1,1),  50 },
                    {  6,  6, new DateOnly(2027,1,1),  22 },
                    {  7,  7, new DateOnly(2027,1,1),  14 },
                    {  8,  8, new DateOnly(2027,1,1),  12 },
                    {  9,  9, new DateOnly(2027,1,1),  16 },
                    { 10, 10, new DateOnly(2027,1,1),  28 },
                    { 11, 11, new DateOnly(2027,1,1),  20 },
                    { 12, 12, new DateOnly(2027,1,1),  17 },
                    { 13, 13, new DateOnly(2027,1,1),  30 },
                    { 14, 14, new DateOnly(2027,1,1),  19 },
                    { 15, 15, new DateOnly(2027,1,1),  21 },
                    { 16, 16, new DateOnly(2027,1,1),  40 },
                    { 17, 17, new DateOnly(2027,1,1),  35 },
                    { 18, 18, new DateOnly(2027,1,1),  50 },
                    { 19, 19, new DateOnly(2027,1,1),  60 },
                    { 20, 20, new DateOnly(2027,1,1),  45 },
                    { 21, 21, new DateOnly(2027,1,1),  30 },
                    { 22, 22, new DateOnly(2027,1,1),  25 },
                    { 23, 23, new DateOnly(2027,1,1),  40 },
                    { 24, 24, new DateOnly(2027,1,1),  55 },
                    { 25, 25, new DateOnly(2027,1,1),  20 },
                    { 26, 26, new DateOnly(2027,1,1),  15 },
                    { 27, 27, new DateOnly(2027,1,1),  15 },
                    { 28, 28, new DateOnly(2027,1,1),  22 },
                    { 29, 29, new DateOnly(2027,1,1),  80 },
                    { 30, 30, new DateOnly(2027,1,1),  40 },
                    { 31, 31, new DateOnly(2027,1,1),  30 },
                    { 32, 32, new DateOnly(2027,1,1), 100 },
                    { 33, 33, new DateOnly(2027,1,1),  45 },
                    { 34, 34, new DateOnly(2027,1,1),  30 },
                    { 35, 35, new DateOnly(2027,1,1),  25 }
                });

            // ── 18. ItemSubstances ────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "ItemSubstances",
                columns: new[] { "Id", "ItemId", "SubstanceId", "Concentration" },
                values: new object[,]
                {
                    {  1,  1,  1, 400f  },  // Nurofen -> Ibuprofen
                    {  2,  2,  2, 500f  },  // Panadol -> Paracetamol
                    {  3,  3,  3, 150f  },  // Magne B6 -> Magnesium
                    {  4,  4,  6,  14f  },  // Feroglobin -> Iron
                    {  5,  5,  4, 1000f },  // Vitamin C 1000 -> Vitamin C
                    {  6,  6,  7, 500f  },  // Calcium + D3 -> Calcium
                    {  7,  6, 17,  10f  },  // Calcium + D3 -> Vitamin D3
                    {  8,  7,  8, 500f  },  // Omega 3 -> Omega 3
                    {  9,  8,  9,   3f  },  // Melatonin -> Melatonin
                    { 10,  9, 10, 100f  },  // Probiotic -> Probiotics
                    { 11, 10, 11,  10f  },  // Zinc -> Zinc
                    { 12, 14,  3, 150f  },  // Femina -> Magnesium
                    { 13, 15, 10, 100f  },  // Herbal Relax -> Probiotics
                    { 14, 16,  5,  10f  },  // Zyrtec -> Cetirizine
                    { 15, 17, 12,  10f  },  // Claritine -> Loratadine
                    { 16, 18, 13,   2f  },  // Imodium -> Loperamide
                    { 17, 19, 14,  40f  },  // Espumisan -> Simethicone
                    { 18, 22, 15,  10f  },  // Voltaren -> Diclofenac
                    { 19, 23, 16,  50f  },  // Bepanthen -> Dexpanthenol
                    { 20, 29, 17,  10f  },  // Vitamin D3 -> Vitamin D3
                    { 21, 33, 18,   0.1f},  // Olynth -> Xylometazoline
                    { 22, 34, 19, 600f  },  // ACC 600 -> Acetylcysteine
                    { 23, 36,  2, 500f  }   // Paracetamol Generic -> Paracetamol
                });

            // ── 19. Orders ────────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "ClientId", "IsCompleted", "IsExpired", "PickUpDate" },
                values: new object[,]
                {
                    { 1, 2, true,  false, new DateOnly(2026,4,15) },
                    { 2, 3, false, false, new DateOnly(2026,4,25) },
                    { 3, 2, false, true,  new DateOnly(2026,3,10) }
                });

            // ── 20. OrderItems ────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "OrderId", "ItemId", "OrderQuantity", "Price" },
                values: new object[,]
                {
                    { 1, 1,  1, 2, 28.5f  },
                    { 2, 1,  5, 1, 22f    },
                    { 3, 2, 14, 1, 29.5f  },
                    { 4, 2, 15, 2, 23.5f  },
                    { 5, 3, 11, 1, 31f    }
                });

            // ── 21. UserDiscounts ─────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "UserDiscounts",
                columns: new[] { "Id", "UserId", "ItemId", "DiscountPercentage" },
                values: new object[,]
                {
                    { 1, 2,  1, 5f   },
                    { 2, 3, 14, 15f  }
                });

            // ── 22. UserNotifications ─────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "UserNotifications",
                columns: new[] { "Id", "UserId", "ItemId", "IsFavorite", "IsStockAlert", "Message" },
                values: new object[,]
                {
                    { 1, 2,  5, true,  false, "Vitamin C 1000 is in your favorites" },
                    { 2, 2, 11, false, true,  "Coldrex MaxGrip stock alert"          },
                    { 3, 3, 14, true,  true,  "Femina Comfort favorite & stock alert" }
                });

            // ── 23. PeriodNotes ───────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "PeriodNotes",
                columns: new[] { "Id", "UserId", "NoteId", "NoteBody", "IsDone" },
                values: new object[,]
                {
                    { 1, 3, 1, "Take magnesium supplement", true  },
                    { 2, 3, 2, "Drink herbal relax tea",     false },
                    { 3, 3, 3, "Buy more Femina Comfort",    false }
                });

            // ── 24. Shifts ────────────────────────────────────────────────────────
            // Status enum as int: 0 = Scheduled
            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "StaffId", "Location", "StartTime", "EndTime", "Status" },
                values: new object[,]
                {
                    { 1, 1, "Clinic",   new DateTime(2026,6,1,9,0,0),  new DateTime(2026,6,1,17,0,0), 0 },
                    { 2, 2, "ER",       new DateTime(2026,6,1,18,0,0), new DateTime(2026,6,1,23,0,0), 0 },
                    { 3, 3, "ER",       new DateTime(2026,6,1,9,0,0),  new DateTime(2026,6,1,17,0,0), 0 },
                    { 4, 4, "Pharmacy", new DateTime(2026,6,1,9,0,0),  new DateTime(2026,6,1,17,0,0), 0 },
                    { 5, 5, "Pharmacy", new DateTime(2026,6,1,18,0,0), new DateTime(2026,6,1,23,0,0), 0 }
                });

            // ── 25. Notifications ─────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "StaffId", "Title", "Message", "ActionButtonText", "IsRead", "CreatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Shift Reminder",  "Your shift starts tomorrow at 09:00.", "View Shift",    false, new DateTime(2026,5,31,8,0,0)  },
                    { 2, 2, "Shift Reminder",  "Your shift starts tomorrow at 18:00.", "View Shift",    false, new DateTime(2026,5,31,8,0,0)  },
                    { 3, 3, "New Evaluation",  "You have a new patient evaluation.",   "View Eval",     false, new DateTime(2026,5,30,10,0,0) }
                });

            // ── 26. Hangouts ──────────────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Hangouts",
                columns: new[] { "HangoutID", "OrganizerId", "Title", "Description", "Location", "Date", "MaxParticipants" },
                values: new object[,]
                {
                    { 1, 1, "Department Lunch",   "Monthly team lunch",           "Hospital Cafeteria", new DateTime(2026,6,5,12,0,0),  20 },
                    { 2, 2, "After-shift Drinks", "Casual get-together after ER", "Local Bar",          new DateTime(2026,6,6,20,0,0),  10 }
                });

            // ── 27. HangoutParticipants ───────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "HangoutParticipants",
                columns: new[] { "Id", "HangoutId", "StaffId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 1, 3 },
                    { 4, 2, 2 },
                    { 5, 2, 3 }
                });

            // ── 28. MedicalEvaluations ────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "MedicalEvaluations",
                columns: new[] { "EvaluationID", "EvaluatorId", "PatientId", "Symptoms", "MedicationsList", "Notes", "EvaluationDate" },
                values: new object[,]
                {
                    {  1, 1, "PAT-001", "Headache, fever, fatigue",        "Panadol Extra",                 "Patient advised to rest and hydrate",    new DateTime(2026,4,10,9,0,0)  },
                    {  2, 1, "PAT-002", "Joint pain, swelling",            "Nurofen Express",               "Referred for physiotherapy",             new DateTime(2026,4,11,10,30,0)},
                    {  3, 2, "PAT-003", "Nausea, abdominal pain",          "Espumisan",                     "Follow-up in 1 week",                    new DateTime(2026,4,12,11,0,0) },
                    {  4, 3, "PAT-004", "Shortness of breath, chest pain", "None",                          "Urgent cardiology referral",             new DateTime(2026,4,13,8,0,0)  },
                    {  5, 1, "PAT-005", "Skin rash, itching",              "Zyrtec",                        "Allergy testing recommended",            new DateTime(2026,4,14,14,0,0) },
                    {  6, 2, "PAT-006", "Insomnia, anxiety",               "Melatonin Sleep",               "Stress management counseling suggested", new DateTime(2026,4,15,15,30,0)},
                    {  7, 3, "PAT-007", "Runny nose, sneezing",            "Claritine, Olynth Nasal Spray", "Seasonal allergies, reassess in spring", new DateTime(2026,4,16,9,30,0) },
                    {  8, 1, "PAT-008", "Back pain, muscle stiffness",     "Voltaren Gel, Nurofen Express", "Advised to avoid heavy lifting",         new DateTime(2026,4,17,11,0,0) },
                    {  9, 2, "PAT-009", "Diarrhea, stomach cramps",        "Imodium, Smecta",               "Dietary changes recommended",            new DateTime(2026,4,18,10,0,0) },
                    { 10, 3, "PAT-010", "Productive cough, mucus",         "ACC 600",                       "Follow-up if no improvement in 5 days",  new DateTime(2026,4,19,13,0,0) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete in reverse dependency order
            migrationBuilder.DeleteData(table: "MedicalEvaluations", keyColumn: "EvaluationID", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10 });
            migrationBuilder.DeleteData(table: "HangoutParticipants", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5 });
            migrationBuilder.DeleteData(table: "Hangouts", keyColumn: "HangoutID", keyValues: new object[] { 1,2 });
            migrationBuilder.DeleteData(table: "Notifications", keyColumn: "Id", keyValues: new object[] { 1,2,3 });
            migrationBuilder.DeleteData(table: "Shifts", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5 });
            migrationBuilder.DeleteData(table: "PeriodNotes", keyColumn: "Id", keyValues: new object[] { 1,2,3 });
            migrationBuilder.DeleteData(table: "UserNotifications", keyColumn: "Id", keyValues: new object[] { 1,2,3 });
            migrationBuilder.DeleteData(table: "UserDiscounts", keyColumn: "Id", keyValues: new object[] { 1,2 });
            migrationBuilder.DeleteData(table: "OrderItems", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5 });
            migrationBuilder.DeleteData(table: "Orders", keyColumn: "Id", keyValues: new object[] { 1,2,3 });
            migrationBuilder.DeleteData(table: "ItemSubstances", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23 });
            migrationBuilder.DeleteData(table: "ItemBatches", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35 });
            migrationBuilder.DeleteData(table: "Items", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,100,101,102,103,104 });
            migrationBuilder.DeleteData(table: "TransferLogs", keyColumn: "TransferLogId", keyValues: new object[] { 1,2,3,4 });
            migrationBuilder.DeleteData(table: "PrescriptionItems", keyColumn: "PrescriptionItemId", keyValues: new object[] { 1,2,3,4,5,6 });
            migrationBuilder.DeleteData(table: "Prescriptions", keyColumn: "PrescriptionId", keyValues: new object[] { 1,2,3,4,5 });
            migrationBuilder.DeleteData(table: "MedicalRecords", keyColumn: "RecordId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10 });
            migrationBuilder.DeleteData(table: "PatientAllergies", keyColumns: new[] { "MedicalHistoryId", "AllergyId" }, keyValues: new object[,]
            {
                { 1,1 },{ 2,2 },{ 3,3 },{ 4,4 },{ 5,5 },{ 6,7 },{ 7,8 },{ 8,9 },{ 9,1 },{ 10,6 }
            });
            migrationBuilder.DeleteData(table: "MedicalHistories", keyColumn: "MedicalHistoryId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.DeleteData(table: "TriageParameters", keyColumn: "TriageParametersId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.DeleteData(table: "Triages", keyColumn: "TriageId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.DeleteData(table: "Examinations", keyColumn: "ExaminationId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10 });
            migrationBuilder.DeleteData(table: "ERRooms", keyColumn: "RoomId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.DeleteData(table: "ERVisits", keyColumn: "VisitId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.DeleteData(table: "Patients", keyColumn: "PatientId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
            migrationBuilder.Sql("DELETE FROM [Staff] WHERE [StaffID] IN (1,2,3,4,5)");
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5,6,7 });

            // Reference data
            migrationBuilder.DeleteData(table: "Substances", keyColumn: "Id", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 });
            migrationBuilder.DeleteData(table: "HighRiskMedicines", keyColumn: "Id", keyValues: new object[] { 1,2 });
            migrationBuilder.DeleteData(table: "Allergies", keyColumn: "AllergyId", keyValues: new object[] { 1,2,3,4,5,6,7,8,9,10 });
        }
    }
}
