namespace Hospital.Data.Models;

public class Pharmacyst : Staff
{
    private const string PharmacistRole = "Pharmacist";

    public Pharmacyst()
    {
        Role = PharmacistRole;
    }
}
