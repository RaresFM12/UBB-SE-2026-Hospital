using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdditionalDemoCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @ClientUserId int = (SELECT TOP 1 [Id] FROM [Users] WHERE [Email] = 'client@gmail.com');
                DECLARE @JohnUserId int = (SELECT TOP 1 [Id] FROM [Users] WHERE [Email] = 'johndoe@test.com');
                DECLARE @JaneUserId int = (SELECT TOP 1 [Id] FROM [Users] WHERE [Email] = 'janedoe@test.com');

                IF @ClientUserId IS NOT NULL
                BEGIN
                    INSERT INTO [BasketEntries] ([UserId], [ItemId], [Quantity], [ExtraDiscountPercentage])
                    SELECT @ClientUserId, [Items].[Id], [Seed].[Quantity], [Seed].[ExtraDiscountPercentage]
                    FROM (VALUES
                        ('Nurofen Express', 2, 5.0),
                        ('Panadol Extra', 1, 0.0),
                        ('Vitamin C 1000', 3, 10.0),
                        ('Probiotic Balance', 1, 0.0),
                        ('Melatonin Sleep', 2, 7.5)
                    ) AS [Seed]([ItemName], [Quantity], [ExtraDiscountPercentage])
                    INNER JOIN [Items] ON [Items].[Name] = [Seed].[ItemName]
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [BasketEntries]
                        WHERE [BasketEntries].[UserId] = @ClientUserId
                          AND [BasketEntries].[ItemId] = [Items].[Id]);
                END

                IF @JohnUserId IS NOT NULL
                BEGIN
                    INSERT INTO [BasketEntries] ([UserId], [ItemId], [Quantity], [ExtraDiscountPercentage])
                    SELECT @JohnUserId, [Items].[Id], [Seed].[Quantity], [Seed].[ExtraDiscountPercentage]
                    FROM (VALUES
                        ('Magne B6', 2, 0.0),
                        ('Coldrex MaxGrip', 1, 3.5),
                        ('Strepsils Intensive', 2, 0.0)
                    ) AS [Seed]([ItemName], [Quantity], [ExtraDiscountPercentage])
                    INNER JOIN [Items] ON [Items].[Name] = [Seed].[ItemName]
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [BasketEntries]
                        WHERE [BasketEntries].[UserId] = @JohnUserId
                          AND [BasketEntries].[ItemId] = [Items].[Id]);
                END

                IF @JaneUserId IS NOT NULL
                BEGIN
                    INSERT INTO [BasketEntries] ([UserId], [ItemId], [Quantity], [ExtraDiscountPercentage])
                    SELECT @JaneUserId, [Items].[Id], [Seed].[Quantity], [Seed].[ExtraDiscountPercentage]
                    FROM (VALUES
                        ('No-Spa Forte', 1, 4.0),
                        ('Femina Comfort', 2, 8.0),
                        ('Herbal Relax Tea Capsules', 1, 0.0)
                    ) AS [Seed]([ItemName], [Quantity], [ExtraDiscountPercentage])
                    INNER JOIN [Items] ON [Items].[Name] = [Seed].[ItemName]
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [BasketEntries]
                        WHERE [BasketEntries].[UserId] = @JaneUserId
                          AND [BasketEntries].[ItemId] = [Items].[Id]);
                END

                INSERT INTO [Appointments] ([DoctorId], [PatientName], [AppointmentDate], [Status], [Type], [Location], [Notes], [DoctorStaffId])
                SELECT [Staff].[StaffId],
                       [Seed].[PatientName],
                       CONVERT(datetime2, [Seed].[AppointmentDate], 126),
                       [Seed].[Status],
                       [Seed].[Type],
                       [Seed].[Location],
                       [Seed].[Notes],
                       [Staff].[StaffId]
                FROM (VALUES
                    ('doctor@gmail.com', 'Alice Johnson', '2026-06-05T09:00:00', 'Scheduled', 'General consultation', 'Clinic 1', 'Demo seed: first visit and blood pressure review.'),
                    ('doctor@gmail.com', 'David Brown', '2026-06-05T10:30:00', 'Confirmed', 'Follow-up', 'Clinic 1', 'Demo seed: medication response follow-up.'),
                    ('doctor.cardiology@gmail.com', 'Bob Smith', '2026-06-05T11:00:00', 'Scheduled', 'Cardiology consult', 'Cardiology 2', 'Demo seed: ECG and chest pain assessment.'),
                    ('doctor.cardiology@gmail.com', 'Irene Rodriguez', '2026-06-06T09:30:00', 'Completed', 'Cardiology follow-up', 'Cardiology 2', 'Demo seed: post-discharge cardiac review.'),
                    ('doctor.pediatrics@gmail.com', 'Mia Thomas', '2026-06-06T12:00:00', 'Scheduled', 'Pediatric consult', 'Pediatrics 1', 'Demo seed: recurring fever evaluation.'),
                    ('doctor.pediatrics@gmail.com', 'Noah Hernandez', '2026-06-07T08:30:00', 'Confirmed', 'Pediatric checkup', 'Pediatrics 1', 'Demo seed: vaccination and growth chart review.'),
                    ('house@hospital.local', 'Grace Martinez', '2026-06-07T14:00:00', 'Scheduled', 'Diagnostics', 'Diagnostics 3', 'Demo seed: complex symptom differential.'),
                    ('wilson@hospital.local', 'Carol Williams', '2026-06-08T09:00:00', 'Confirmed', 'Oncology consult', 'Oncology 1', 'Demo seed: lab results discussion.'),
                    ('cuddy@hospital.local', 'Karen Anderson', '2026-06-08T13:30:00', 'Scheduled', 'Surgery consult', 'Surgery 2', 'Demo seed: pre-operative review.'),
                    ('doctor@gmail.com', 'Peter Jackson', '2026-06-09T15:00:00', 'Cancelled', 'General consultation', 'Clinic 1', 'Demo seed: patient requested reschedule.'),
                    ('doctor.cardiology@gmail.com', 'Tom Thompson', '2026-06-10T10:00:00', 'Scheduled', 'Cardiology consult', 'Cardiology 2', 'Demo seed: arrhythmia screening.'),
                    ('doctor.pediatrics@gmail.com', 'Sarah Perez', '2026-06-10T11:30:00', 'Completed', 'Pediatric follow-up', 'Pediatrics 1', 'Demo seed: recovery check after ER visit.')
                ) AS [Seed]([DoctorEmail], [PatientName], [AppointmentDate], [Status], [Type], [Location], [Notes])
                INNER JOIN [Staff] ON [Staff].[Email] = [Seed].[DoctorEmail]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [Appointments]
                    WHERE [Appointments].[PatientName] = [Seed].[PatientName]
                      AND [Appointments].[AppointmentDate] = CONVERT(datetime2, [Seed].[AppointmentDate], 126)
                      AND ([Appointments].[DoctorId] = [Staff].[StaffId]
                           OR [Appointments].[DoctorStaffId] = [Staff].[StaffId]));

                INSERT INTO [ERRequests] ([AssignedDoctorId], [Specialization], [Location], [CreatedAt], [Status])
                SELECT [Staff].[StaffId],
                       [Seed].[Specialization],
                       [Seed].[Location],
                       CONVERT(datetime2, [Seed].[CreatedAt], 126),
                       [Seed].[Status]
                FROM (VALUES
                    ('doctor@gmail.com', 'General Medicine', 'ER', '2026-06-03T08:15:00', 'ASSIGNED'),
                    ('doctor.cardiology@gmail.com', 'Cardiology', 'ER', '2026-06-03T08:40:00', 'ASSIGNED'),
                    (NULL, 'Pediatrics', 'ER', '2026-06-03T09:05:00', 'PENDING'),
                    ('house@hospital.local', 'Diagnostics', 'ER', '2026-06-03T09:25:00', 'ASSIGNED'),
                    (NULL, 'Neurology', 'ER', '2026-06-03T10:10:00', 'PENDING'),
                    ('cuddy@hospital.local', 'Surgery', 'ER', '2026-06-03T10:45:00', 'ASSIGNED'),
                    ('doctor.pediatrics@gmail.com', 'Pediatrics', 'ER', '2026-06-03T11:20:00', 'ASSIGNED'),
                    (NULL, 'Cardiology', 'ER', '2026-06-03T12:00:00', 'UNMATCHED')
                ) AS [Seed]([DoctorEmail], [Specialization], [Location], [CreatedAt], [Status])
                LEFT JOIN [Staff] ON [Staff].[Email] = [Seed].[DoctorEmail]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [ERRequests]
                    WHERE [ERRequests].[Specialization] = [Seed].[Specialization]
                      AND [ERRequests].[Location] = [Seed].[Location]
                      AND [ERRequests].[CreatedAt] = CONVERT(datetime2, [Seed].[CreatedAt], 126));

                INSERT INTO [PharmacyHandovers] ([PharmacistId], [HandoverDate], [Notes])
                SELECT [Staff].[StaffId],
                       CONVERT(datetime2, [Seed].[HandoverDate], 126),
                       [Seed].[Notes]
                FROM (VALUES
                    ('pharmacy@gmail.com', '2026-06-04T17:10:00', 'Demo seed: counted controlled medicines, fridge temperature normal, two low-stock alerts open.'),
                    ('pharmacist.compounding@gmail.com', '2026-06-04T17:20:00', 'Demo seed: prepared pediatric suspensions and handed over pending labels.'),
                    ('pharmacist.inventory@gmail.com', '2026-06-04T17:30:00', 'Demo seed: inventory reorder list created for antibiotics and supplements.'),
                    ('jamie@hospital.local', '2026-06-01T17:15:00', 'Demo seed: day shift closed with five prescriptions ready for pickup.'),
                    ('pat@hospital.local', '2026-06-01T23:05:00', 'Demo seed: evening shift closed, urgent stock request sent to supplier.')
                ) AS [Seed]([PharmacistEmail], [HandoverDate], [Notes])
                INNER JOIN [Staff] ON [Staff].[Email] = [Seed].[PharmacistEmail]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [PharmacyHandovers]
                    WHERE [PharmacyHandovers].[PharmacistId] = [Staff].[StaffId]
                      AND [PharmacyHandovers].[HandoverDate] = CONVERT(datetime2, [Seed].[HandoverDate], 126));

                INSERT INTO [ShiftSwapRequests] ([ShiftId], [RequestingStaffId], [TargetStaffId], [RequestedAt], [Status])
                SELECT [Shifts].[Id],
                       [Requester].[StaffId],
                       [Target].[StaffId],
                       CONVERT(datetime2, [Seed].[RequestedAt], 126),
                       [Seed].[Status]
                FROM (VALUES
                    ('doctor@gmail.com', 'doctor@gmail.com', 'doctor.cardiology@gmail.com', '2026-06-04T08:00:00', '2026-06-03T13:00:00', 0),
                    ('doctor.cardiology@gmail.com', 'doctor.cardiology@gmail.com', 'doctor.pediatrics@gmail.com', '2026-06-04T08:00:00', '2026-06-03T13:15:00', 1),
                    ('pharmacy@gmail.com', 'pharmacy@gmail.com', 'pharmacist.compounding@gmail.com', '2026-06-04T09:00:00', '2026-06-03T14:00:00', 0),
                    ('pharmacist.compounding@gmail.com', 'pharmacist.compounding@gmail.com', 'pharmacist.inventory@gmail.com', '2026-06-04T09:00:00', '2026-06-03T14:30:00', 2),
                    ('house@hospital.local', 'house@hospital.local', 'wilson@hospital.local', '2026-06-01T09:00:00', '2026-05-31T16:45:00', 3)
                ) AS [Seed]([ShiftOwnerEmail], [RequesterEmail], [TargetEmail], [ShiftStart], [RequestedAt], [Status])
                INNER JOIN [Staff] AS [ShiftOwner] ON [ShiftOwner].[Email] = [Seed].[ShiftOwnerEmail]
                INNER JOIN [Shifts] ON [Shifts].[StaffId] = [ShiftOwner].[StaffId]
                    AND [Shifts].[StartTime] = CONVERT(datetime2, [Seed].[ShiftStart], 126)
                INNER JOIN [Staff] AS [Requester] ON [Requester].[Email] = [Seed].[RequesterEmail]
                INNER JOIN [Staff] AS [Target] ON [Target].[Email] = [Seed].[TargetEmail]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [ShiftSwapRequests]
                    WHERE [ShiftSwapRequests].[ShiftId] = [Shifts].[Id]
                      AND [ShiftSwapRequests].[RequestingStaffId] = [Requester].[StaffId]
                      AND [ShiftSwapRequests].[TargetStaffId] = [Target].[StaffId]
                      AND [ShiftSwapRequests].[RequestedAt] = CONVERT(datetime2, [Seed].[RequestedAt], 126));

                INSERT INTO [Transplants] ([ReceiverId], [DonorId], [OrganType], [RequestDate], [TransplantDate], [Status], [CompatibilityScore])
                SELECT [Seed].[ReceiverId],
                       [Seed].[DonorId],
                       [Seed].[OrganType],
                       CONVERT(datetime2, [Seed].[RequestDate], 126),
                       CASE WHEN [Seed].[TransplantDate] IS NULL THEN NULL ELSE CONVERT(datetime2, [Seed].[TransplantDate], 126) END,
                       [Seed].[Status],
                       [Seed].[CompatibilityScore]
                FROM (VALUES
                    (1, 2, 'Kidney', '2026-05-01T09:00:00', NULL, 0, 0.73),
                    (3, 5, 'Liver', '2026-04-12T10:30:00', '2026-06-20T08:00:00', 2, 0.88),
                    (6, 8, 'Heart', '2026-03-18T15:45:00', NULL, 1, 0.81),
                    (9, 12, 'Lung', '2026-02-10T12:00:00', '2026-05-25T07:30:00', 3, 0.91),
                    (11, NULL, 'Kidney', '2026-05-22T11:15:00', NULL, 0, 0.64),
                    (13, 15, 'Cornea', '2026-05-28T14:20:00', '2026-06-18T09:00:00', 2, 0.79)
                ) AS [Seed]([ReceiverId], [DonorId], [OrganType], [RequestDate], [TransplantDate], [Status], [CompatibilityScore])
                INNER JOIN [Patients] AS [Receiver] ON [Receiver].[PatientId] = [Seed].[ReceiverId]
                LEFT JOIN [Patients] AS [Donor] ON [Donor].[PatientId] = [Seed].[DonorId]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [Transplants]
                    WHERE [Transplants].[ReceiverId] = [Seed].[ReceiverId]
                      AND [Transplants].[OrganType] = [Seed].[OrganType]
                      AND [Transplants].[RequestDate] = CONVERT(datetime2, [Seed].[RequestDate], 126));

                INSERT INTO [TransplantMatches] ([TransplantId], [ReceiverId], [ReceiverName], [BloodType], [CompatibilityScore], [RequestDate], [WaitingDays])
                SELECT [Transplants].[TransplantId],
                       [Seed].[ReceiverId],
                       [Seed].[ReceiverName],
                       [Seed].[BloodType],
                       [Seed].[CompatibilityScore],
                       CONVERT(datetime2, [Seed].[MatchRequestDate], 126),
                       [Seed].[WaitingDays]
                FROM (VALUES
                    (1, 'Kidney', '2026-05-01T09:00:00', 'Alice Johnson', 'A+', 0.73, '2026-06-03T08:00:00', 33),
                    (3, 'Liver', '2026-04-12T10:30:00', 'Carol Williams', 'O+', 0.88, '2026-06-03T08:05:00', 52),
                    (6, 'Heart', '2026-03-18T15:45:00', 'Frank Garcia', 'B-', 0.81, '2026-06-03T08:10:00', 77),
                    (9, 'Lung', '2026-02-10T12:00:00', 'Irene Rodriguez', 'AB+', 0.91, '2026-06-03T08:15:00', 113),
                    (11, 'Kidney', '2026-05-22T11:15:00', 'Karen Anderson', 'A-', 0.64, '2026-06-03T08:20:00', 12),
                    (13, 'Cornea', '2026-05-28T14:20:00', 'Mia Thomas', 'O-', 0.79, '2026-06-03T08:25:00', 6)
                ) AS [Seed]([ReceiverId], [OrganType], [TransplantRequestDate], [ReceiverName], [BloodType], [CompatibilityScore], [MatchRequestDate], [WaitingDays])
                INNER JOIN [Transplants] ON [Transplants].[ReceiverId] = [Seed].[ReceiverId]
                    AND [Transplants].[OrganType] = [Seed].[OrganType]
                    AND [Transplants].[RequestDate] = CONVERT(datetime2, [Seed].[TransplantRequestDate], 126)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [TransplantMatches]
                    WHERE [TransplantMatches].[TransplantId] = [Transplants].[TransplantId]
                      AND [TransplantMatches].[ReceiverId] = [Seed].[ReceiverId]
                      AND [TransplantMatches].[RequestDate] = CONVERT(datetime2, [Seed].[MatchRequestDate], 126));
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [TransplantMatches]
                WHERE [RequestDate] IN (
                    CONVERT(datetime2, '2026-06-03T08:00:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:05:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:10:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:15:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:20:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:25:00', 126));

                DELETE FROM [Transplants]
                WHERE ([ReceiverId] = 1 AND [OrganType] = 'Kidney' AND [RequestDate] = CONVERT(datetime2, '2026-05-01T09:00:00', 126))
                   OR ([ReceiverId] = 3 AND [OrganType] = 'Liver' AND [RequestDate] = CONVERT(datetime2, '2026-04-12T10:30:00', 126))
                   OR ([ReceiverId] = 6 AND [OrganType] = 'Heart' AND [RequestDate] = CONVERT(datetime2, '2026-03-18T15:45:00', 126))
                   OR ([ReceiverId] = 9 AND [OrganType] = 'Lung' AND [RequestDate] = CONVERT(datetime2, '2026-02-10T12:00:00', 126))
                   OR ([ReceiverId] = 11 AND [OrganType] = 'Kidney' AND [RequestDate] = CONVERT(datetime2, '2026-05-22T11:15:00', 126))
                   OR ([ReceiverId] = 13 AND [OrganType] = 'Cornea' AND [RequestDate] = CONVERT(datetime2, '2026-05-28T14:20:00', 126));

                DELETE FROM [ShiftSwapRequests]
                WHERE [RequestedAt] IN (
                    CONVERT(datetime2, '2026-06-03T13:00:00', 126),
                    CONVERT(datetime2, '2026-06-03T13:15:00', 126),
                    CONVERT(datetime2, '2026-06-03T14:00:00', 126),
                    CONVERT(datetime2, '2026-06-03T14:30:00', 126),
                    CONVERT(datetime2, '2026-05-31T16:45:00', 126));

                DELETE FROM [PharmacyHandovers]
                WHERE [Notes] LIKE 'Demo seed:%';

                DELETE FROM [ERRequests]
                WHERE [CreatedAt] IN (
                    CONVERT(datetime2, '2026-06-03T08:15:00', 126),
                    CONVERT(datetime2, '2026-06-03T08:40:00', 126),
                    CONVERT(datetime2, '2026-06-03T09:05:00', 126),
                    CONVERT(datetime2, '2026-06-03T09:25:00', 126),
                    CONVERT(datetime2, '2026-06-03T10:10:00', 126),
                    CONVERT(datetime2, '2026-06-03T10:45:00', 126),
                    CONVERT(datetime2, '2026-06-03T11:20:00', 126),
                    CONVERT(datetime2, '2026-06-03T12:00:00', 126));

                DELETE FROM [Appointments]
                WHERE [Notes] LIKE 'Demo seed:%';

                DELETE [BasketEntries]
                FROM [BasketEntries]
                INNER JOIN [Users] ON [Users].[Id] = [BasketEntries].[UserId]
                INNER JOIN [Items] ON [Items].[Id] = [BasketEntries].[ItemId]
                WHERE [Users].[Email] IN ('client@gmail.com', 'johndoe@test.com', 'janedoe@test.com')
                  AND [Items].[Name] IN (
                    'Nurofen Express',
                    'Panadol Extra',
                    'Vitamin C 1000',
                    'Probiotic Balance',
                    'Melatonin Sleep',
                    'Magne B6',
                    'Coldrex MaxGrip',
                    'Strepsils Intensive',
                    'No-Spa Forte',
                    'Femina Comfort',
                    'Herbal Relax Tea Capsules');
                """);

        }
    }
}
