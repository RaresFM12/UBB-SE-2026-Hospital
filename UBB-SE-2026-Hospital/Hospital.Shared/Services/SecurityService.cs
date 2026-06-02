namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public class SecurityService : ISecurityService
    {
        public string HashPassword(string password)
        {
            return password;
        }

        public bool VerifyPassword(string password, string stored)
        {
            return password == stored;
        }
    }
}
