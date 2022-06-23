using IdentityModel;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace DI.TokenService.Store
{
    public class CustomRopValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserRepository _userRepository;

        public CustomRopValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var vr = await _userRepository.ValidateCredentials(context.UserName, context.Password);
            if (vr)
            {
                var user = _userRepository.FindByUsername(context.UserName);
                context.Result = new GrantValidationResult($"{user.Id}", OidcConstants.AuthenticationMethods.Password);
            }
        }
    }
}
