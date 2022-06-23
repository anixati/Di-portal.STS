using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

using System.Security.Claims;
using IdentityServer4.Extensions;
using System.Collections.Generic;



namespace DI.TokenService.Store
{
    public class CustomProfileService : IProfileService
    {
        protected readonly ILogger Logger;
        protected readonly IUserRepository _userRepository;

        public CustomProfileService(IUserRepository userRepository, ILogger<CustomProfileService> logger)
        {
            _userRepository = userRepository;
            Logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
          

            context.IssuedClaims = await _userRepository.GetClaims(subjectId);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userRepository.FindBySubjectId(sub);
            context.IsActive = user != null;
        }

    }
}

