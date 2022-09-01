using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DI.TokenService.Store
{
    public interface IUserRepository
    {
        Task<bool> ValidateCredentials(string username, string password);
        Task<CustomUser> FindBySubjectId(string subjectId);
        Task<CustomUser> FindByUsername(string username);
        Task<CustomUser> FindByExternalProvider(string provider, string providerUserId, List<Claim> claims);
        Task<CustomUser> AutoProvisionUser(string provider, string providerUserId, List<Claim> claims);
        Task<List<Claim>> GetClaims(string subjectId);
    }
}
