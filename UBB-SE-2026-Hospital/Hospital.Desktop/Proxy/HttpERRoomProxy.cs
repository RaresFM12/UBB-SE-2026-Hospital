using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpERRoomProxy(HttpClient httpClient) : ProxyBase(httpClient), IERRoomService
{
    private const string BaseUri = "api/er-rooms";

    public async Task<List<ERRoom>> GetAllAsync()
        => await GetAsync<List<ERRoom>>(BaseUri) ?? [];

    public async Task<ERRoom?> GetByIdAsync(int id)
        => await GetAsync<ERRoom>($"{BaseUri}/{id}");

    public async Task<ERRoom> CreateAsync(ERRoom room)
        => await PostAsync<ERRoom, ERRoom>(BaseUri, room) ?? room;

    public async Task<ERRoom> UpdateAsync(ERRoom room)
    {
        await PutAsync($"{BaseUri}/{room.RoomId}", room);
        return room;
    }

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<List<ERRoom>> GetByStatusAsync(string status)
        => await GetAsync<List<ERRoom>>($"{BaseUri}/status/{status}") ?? [];

    public async Task<ERRoomVisitDetails?> GetVisitDetailsAsync(int roomId)
        => await GetAsync<ERRoomVisitDetails>($"{BaseUri}/{roomId}/visit-details");

    public async Task MarkRoomAsCleaningAsync(int roomId)
        => await PostAsync<object, object>($"{BaseUri}/{roomId}/mark-cleaning", new { });

    public async Task MarkRoomAsAvailableAsync(int roomId)
        => await PostAsync<object, object>($"{BaseUri}/{roomId}/mark-available", new { });
}
