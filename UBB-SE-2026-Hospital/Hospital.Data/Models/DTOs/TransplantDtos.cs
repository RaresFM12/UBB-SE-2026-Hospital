namespace Hospital.Data.Models.DTOs;

public class CreateWaitlistRequest
{
    public int ReceiverId { get; set; }
    public string OrganType { get; set; } = string.Empty;
}

public class AssignDonorRequest
{
    public int DonorId { get; set; }
    public float FinalScore { get; set; }
}
