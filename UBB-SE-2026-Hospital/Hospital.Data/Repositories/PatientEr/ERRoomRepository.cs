using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ERRoomRepository(HospitalDbContext context) : IERRoomRepository
{
    public async Task<ERRoom?> GetByIdAsync(int roomId)
        => await context.ERRooms.FindAsync(roomId);

    public async Task<List<ERRoom>> GetAllAsync()
        => await context.ERRooms.ToListAsync();

    public async Task<List<ERRoom>> GetAvailableRoomsAsync()
    {
        bool IsAvailableRoom(ERRoom room) => room.AvailabilityStatus == ERRoom.RoomStatus.Available;
        var all = await context.ERRooms.ToListAsync();
        return all.Where(IsAvailableRoom).ToList();
    }

    public async Task<ERRoom> CreateAsync(ERRoom room)
    {
        context.ERRooms.Add(room);
        await context.SaveChangesAsync();
        return room;
    }

    public async Task<ERRoom> UpdateAsync(ERRoom room)
    {
        context.ERRooms.Update(room);
        await context.SaveChangesAsync();
        return room;
    }

    public async Task DeleteAsync(int roomId)
    {
        var room = await context.ERRooms.FindAsync(roomId);
        if (room is not null)
        {
            context.ERRooms.Remove(room);
            await context.SaveChangesAsync();
        }
    }
}
