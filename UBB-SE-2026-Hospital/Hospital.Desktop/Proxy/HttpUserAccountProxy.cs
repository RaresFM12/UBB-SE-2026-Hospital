using Hospital.Shared.Models;
using Hospital.Shared.Services;
using System.Net.Http.Json;

namespace Hospital.Desktop.Proxy;

public class HttpUserAccountProxy(HttpClient httpClient) : ProxyBase(httpClient), IUserAccountService
{
    private const string BaseUri = "api/users";

    public User? CurrentUser { get; private set; }

    public User? LoadCurrentUser(int userId)
    {
        var user = Task.Run(async () => await GetAsync<User>($"{BaseUri}/{userId}")).GetAwaiter().GetResult();
        CurrentUser = user;
        return user;
    }

    public void Login(string email, string password)
    {
        throw new NotSupportedException("Use AuthClient for login.");
    }

    public void Register(string email, string password, string confirmPassword, string username, string phoneNumber, string role = "Client")
    {
        Task.Run(async () => await PostAsync<object, object>(BaseUri, new { email, password, confirmPassword, username, phoneNumber, role })).GetAwaiter().GetResult();
    }

    public void UpdateProfile(string newUsername, string newPhoneNumber)
    {
        if (CurrentUser == null) return;
        CurrentUser.Username = newUsername;
        CurrentUser.PhoneNumber = newPhoneNumber;
        Task.Run(async () => await PutAsync($"{BaseUri}/{CurrentUser.Id}", CurrentUser)).GetAwaiter().GetResult();
    }

    public void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
    {
        throw new NotSupportedException("Password changes are handled by AuthController.");
    }

    public List<User> SearchUsers(string query)
    {
        var url = string.IsNullOrWhiteSpace(query) ? BaseUri : $"{BaseUri}/search?q={Uri.EscapeDataString(query)}";
        return Task.Run(async () => await GetAsync<List<User>>(url) ?? []).GetAwaiter().GetResult();
    }

    public void PromoteToAdmin(User client)
    {
        Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{client.Id}/promote", new { })).GetAwaiter().GetResult();
    }

    public void DisableAccount(User client)
    {
        Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{client.Id}/disable", new { })).GetAwaiter().GetResult();
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}
