namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IShiftSwapRepository
    {
        int AddShiftSwapRequest(ShiftSwapRequest request);

        IReadOnlyList<ShiftSwapRequest> GetAllShiftSwapRequests();

        ShiftSwapRequest? GetShiftSwapRequestById(int swapId);

        void UpdateShiftSwapRequestStatus(int swapId, string status);
    }
}
