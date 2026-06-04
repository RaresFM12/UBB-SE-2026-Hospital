using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedRequestedDemoAccountsAndStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'admin@gmail.com')
                BEGIN
                    UPDATE [Users]
                    SET [PhoneNumber] = '0700000101',
                        [PasswordHash] = 'Pussycats1!',
                        [IsDisabled] = 0,
                        [IsAdmin] = 1,
                        [Username] = 'Admin',
                        [Role] = 'Admin',
                        [DiscountNotifications] = 1,
                        [LoyaltyPoints] = 1000,
                        [StartPeriodDate] = '19000101',
                        [CycleDays] = 28,
                        [PeriodLasts] = 5,
                        [PremenstrualSyndromeOption] = 0
                    WHERE [Email] = 'admin@gmail.com';
                END
                ELSE
                BEGIN
                    INSERT INTO [Users]
                        ([Email], [PhoneNumber], [PasswordHash], [IsDisabled], [IsAdmin], [Username], [Role],
                         [DiscountNotifications], [LoyaltyPoints], [StartPeriodDate], [CycleDays], [PeriodLasts], [PremenstrualSyndromeOption])
                    VALUES
                        ('admin@gmail.com', '0700000101', 'Pussycats1!', 0, 1, 'Admin', 'Admin',
                         1, 1000, '19000101', 28, 5, 0);
                END

                IF EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'client@gmail.com')
                BEGIN
                    UPDATE [Users]
                    SET [PhoneNumber] = '0700000102',
                        [PasswordHash] = 'Pussycats1!',
                        [IsDisabled] = 0,
                        [IsAdmin] = 0,
                        [Username] = 'Client',
                        [Role] = 'Client',
                        [DiscountNotifications] = 1,
                        [LoyaltyPoints] = 250,
                        [StartPeriodDate] = '19000101',
                        [CycleDays] = 28,
                        [PeriodLasts] = 5,
                        [PremenstrualSyndromeOption] = 0
                    WHERE [Email] = 'client@gmail.com';
                END
                ELSE
                BEGIN
                    INSERT INTO [Users]
                        ([Email], [PhoneNumber], [PasswordHash], [IsDisabled], [IsAdmin], [Username], [Role],
                         [DiscountNotifications], [LoyaltyPoints], [StartPeriodDate], [CycleDays], [PeriodLasts], [PremenstrualSyndromeOption])
                    VALUES
                        ('client@gmail.com', '0700000102', 'Pussycats1!', 0, 0, 'Client', 'Client',
                         1, 250, '19000101', 28, 5, 0);
                END

                IF EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'doctor@gmail.com')
                BEGIN
                    UPDATE [Users]
                    SET [PhoneNumber] = '0700000103',
                        [PasswordHash] = 'Pussycats1!',
                        [IsDisabled] = 0,
                        [IsAdmin] = 0,
                        [Username] = 'Doctor',
                        [Role] = 'Doctor',
                        [DiscountNotifications] = 0,
                        [LoyaltyPoints] = 0,
                        [StartPeriodDate] = '19000101',
                        [CycleDays] = 28,
                        [PeriodLasts] = 5,
                        [PremenstrualSyndromeOption] = 0
                    WHERE [Email] = 'doctor@gmail.com';
                END
                ELSE
                BEGIN
                    INSERT INTO [Users]
                        ([Email], [PhoneNumber], [PasswordHash], [IsDisabled], [IsAdmin], [Username], [Role],
                         [DiscountNotifications], [LoyaltyPoints], [StartPeriodDate], [CycleDays], [PeriodLasts], [PremenstrualSyndromeOption])
                    VALUES
                        ('doctor@gmail.com', '0700000103', 'Pussycats1!', 0, 0, 'Doctor', 'Doctor',
                         0, 0, '19000101', 28, 5, 0);
                END

                IF EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'pharmacy@gmail.com')
                BEGIN
                    UPDATE [Users]
                    SET [PhoneNumber] = '0700000104',
                        [PasswordHash] = 'Pussycats1!',
                        [IsDisabled] = 0,
                        [IsAdmin] = 0,
                        [Username] = 'Pharmacy',
                        [Role] = 'Pharmacist',
                        [DiscountNotifications] = 0,
                        [LoyaltyPoints] = 0,
                        [StartPeriodDate] = '19000101',
                        [CycleDays] = 28,
                        [PeriodLasts] = 5,
                        [PremenstrualSyndromeOption] = 0
                    WHERE [Email] = 'pharmacy@gmail.com';
                END
                ELSE
                BEGIN
                    INSERT INTO [Users]
                        ([Email], [PhoneNumber], [PasswordHash], [IsDisabled], [IsAdmin], [Username], [Role],
                         [DiscountNotifications], [LoyaltyPoints], [StartPeriodDate], [CycleDays], [PeriodLasts], [PremenstrualSyndromeOption])
                    VALUES
                        ('pharmacy@gmail.com', '0700000104', 'Pussycats1!', 0, 0, 'Pharmacy', 'Pharmacist',
                         0, 0, '19000101', 28, 5, 0);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'doctor@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('doctor@gmail.com', 'Pussycats1!', 'Doctor', 'General Medicine', 'Doctor', 'Demo',
                         '0700000103', 1, 'LIC-DEMO-DOCTOR', 'General Medicine', 'Available', 'Board Certified', 7, 145.0, 0);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'pharmacy@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('pharmacy@gmail.com', 'Pussycats1!', 'Pharmacist', 'Pharmacy', 'Pharmacy', 'Demo',
                         '0700000104', 1, 'LIC-DEMO-PHARMACY', 'Pharmacy', 'Available', 'Hospital Pharmacy', 5, 85.0, NULL);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'doctor.cardiology@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('doctor.cardiology@gmail.com', 'Pussycats1!', 'Doctor', 'Cardiology', 'Mara', 'Ionescu',
                         '0700000105', 1, 'LIC-DEMO-CARDIO', 'Cardiology', 'Available', 'Board Certified', 11, 165.0, 0);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'doctor.pediatrics@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('doctor.pediatrics@gmail.com', 'Pussycats1!', 'Doctor', 'Pediatrics', 'Andrei', 'Pop',
                         '0700000106', 1, 'LIC-DEMO-PEDS', 'Pediatrics', 'Available', 'Board Certified', 9, 150.0, 0);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'pharmacist.compounding@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('pharmacist.compounding@gmail.com', 'Pussycats1!', 'Pharmacist', 'Pharmacy', 'Irina', 'Dumitrescu',
                         '0700000107', 1, 'LIC-DEMO-COMPOUND', 'Pharmacy', 'Available', 'Compounding', 8, 92.0, NULL);
                END

                IF NOT EXISTS (SELECT 1 FROM [Staff] WHERE [Email] = 'pharmacist.inventory@gmail.com')
                BEGIN
                    INSERT INTO [Staff]
                        ([Email], [PasswordHash], [Role], [Department], [FirstName], [LastName], [ContactInfo], [Available],
                         [LicenseNumber], [Specialization], [Status], [Certification], [YearsOfExperience], [HourlyRate], [DoctorStatus])
                    VALUES
                        ('pharmacist.inventory@gmail.com', 'Pussycats1!', 'Pharmacist', 'Pharmacy', 'Vlad', 'Marin',
                         '0700000108', 1, 'LIC-DEMO-INVENTORY', 'Pharmacy', 'Available', 'Inventory Control', 6, 88.0, NULL);
                END

                INSERT INTO [Shifts] ([StaffId], [Location], [StartTime], [EndTime], [Status])
                SELECT [StaffId], 'Clinic', '2026-06-04T08:00:00', '2026-06-04T16:00:00', 0
                FROM [Staff]
                WHERE [Email] IN ('doctor@gmail.com', 'doctor.cardiology@gmail.com', 'doctor.pediatrics@gmail.com')
                  AND NOT EXISTS (
                      SELECT 1 FROM [Shifts]
                      WHERE [Shifts].[StaffId] = [Staff].[StaffId]
                        AND [Shifts].[StartTime] = '2026-06-04T08:00:00');

                INSERT INTO [Shifts] ([StaffId], [Location], [StartTime], [EndTime], [Status])
                SELECT [StaffId], 'Pharmacy', '2026-06-04T09:00:00', '2026-06-04T17:00:00', 0
                FROM [Staff]
                WHERE [Email] IN ('pharmacy@gmail.com', 'pharmacist.compounding@gmail.com', 'pharmacist.inventory@gmail.com')
                  AND NOT EXISTS (
                      SELECT 1 FROM [Shifts]
                      WHERE [Shifts].[StaffId] = [Staff].[StaffId]
                        AND [Shifts].[StartTime] = '2026-06-04T09:00:00');
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Shifts]
                WHERE [StaffId] IN (
                    SELECT [StaffId]
                    FROM [Staff]
                    WHERE [Email] IN (
                        'doctor@gmail.com',
                        'pharmacy@gmail.com',
                        'doctor.cardiology@gmail.com',
                        'doctor.pediatrics@gmail.com',
                        'pharmacist.compounding@gmail.com',
                        'pharmacist.inventory@gmail.com'))
                  AND [StartTime] IN ('2026-06-04T08:00:00', '2026-06-04T09:00:00');

                DELETE FROM [Staff]
                WHERE [Email] IN (
                    'doctor@gmail.com',
                    'pharmacy@gmail.com',
                    'doctor.cardiology@gmail.com',
                    'doctor.pediatrics@gmail.com',
                    'pharmacist.compounding@gmail.com',
                    'pharmacist.inventory@gmail.com');

                DELETE FROM [Users]
                WHERE [Email] IN (
                    'admin@gmail.com',
                    'client@gmail.com',
                    'doctor@gmail.com',
                    'pharmacy@gmail.com');
                """);
        }
    }
}
