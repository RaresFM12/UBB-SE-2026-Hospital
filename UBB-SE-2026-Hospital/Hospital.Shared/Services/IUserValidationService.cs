namespace Hospital.Shared.Services;

public interface IUserValidationService
{
    bool IsCorrectEmailFormat(string email);

    bool IsCorrectPasswordFormat(string password);

    bool IsCorrectPhoneNumberFormat(string phoneNumber);

    bool IsCorrectUsernameFormat(string username);
}
