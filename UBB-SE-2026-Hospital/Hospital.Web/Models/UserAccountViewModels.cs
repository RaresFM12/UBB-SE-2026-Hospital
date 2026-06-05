namespace Hospital.Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class UserAccountListViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public List<UserAccountListItemViewModel> Users { get; set; } = new();
    }

    public class UserAccountListItemViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public bool IsDisabled { get; set; }

        public string DisplayRole => string.IsNullOrWhiteSpace(this.Role)
            ? (this.IsAdmin ? "Admin" : "Client")
            : this.Role;

        public string Status => this.IsDisabled ? "Disabled" : (this.IsAdmin ? "Admin" : "Active");

        public bool CanPromote => !this.IsAdmin && !this.IsDisabled;

        public bool CanDisable => !this.IsAdmin && !this.IsDisabled;
    }

    public class UserAccountDetailsViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public bool IsDisabled { get; set; }

        public int LoyaltyPoints { get; set; }

        public string DisplayRole => string.IsNullOrWhiteSpace(this.Role)
            ? (this.IsAdmin ? "Admin" : "Client")
            : this.Role;
    }

    public class UserAccountCreateViewModel
    {
        [Required(ErrorMessage = "E-mail is required.")]
        [RegularExpression(@"^.+@.+\..+", ErrorMessage = "Enter a valid e-mail address (e.g. name@example.com).")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*])[A-Za-z\d!@#%^*]{8,}$",
            ErrorMessage = "Password must be 8+ characters and include uppercase, lowercase, a digit, and a special character (!@#%^*).")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [RegularExpression(@"^[A-Za-z_]+$", ErrorMessage = "Username must contain only letters and underscores.")]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain only digits.")]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; } = "Client";

        public IReadOnlyList<string> RoleOptions { get; } = new List<string>
        {
            "Admin",
            "Client",
            "Doctor",
            "Pharmacist",
        };
    }

    public class UserAccountEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public string? SuccessMessage { get; set; }
    }

    public class UserAccountDeleteViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public bool IsDisabled { get; set; }

        public string DisplayRole => string.IsNullOrWhiteSpace(this.Role)
            ? (this.IsAdmin ? "Admin" : "Client")
            : this.Role;
    }

    public class UserAccountChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*]).+$",
            ErrorMessage = "Password must include uppercase, lowercase, a digit, and a special character (!@#%^*).")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm the new password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
