namespace Hospital.Shared.Services;

public interface ISecurityService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string stored);
}
