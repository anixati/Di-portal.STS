using System;

namespace DI.TokenService.Store
{
    public class CustomUser
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string PasswordHash { get; set; }

        public DateTime? AccessRequest { get; set; }
        public DateTime? AccessGranted { get; set; }
    }
}
