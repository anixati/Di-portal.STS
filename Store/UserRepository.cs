using System;
using Dapper;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using DI.TokenService.Core;
using IdentityModel;

namespace DI.TokenService.Store
{

    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;
        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        private string _insUserSql = @"INSERT INTO [acl].[Users]
                ([UserId],[PasswordHash],[NameId],[Upn],[DisplayName],[AccessRequest],[IsSystem],[Locked],[Disabled],[Deleted],[CreatedOn],[CreatedBy],[ModifiedOn],[ModifiedBy],[FirstName],[LastName],[Gender],[Email1])
        VALUES (@UserId,@PasswordHash,@NameId,@Upn,@DisplayName,@AccessRequest,@IsSystem,@Locked,@Disabled,@Deleted,@CreatedOn,@CreatedBy,@ModifiedOn,@ModifiedBy,@FirstName,@LastName,@Gender,@Email1)";

        public async Task<CustomUser> AutoProvisionUser(string provider, string providerUserId, List<Claim> claims)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(_insUserSql, new
            {
                @UserId=providerUserId,
                @PasswordHash="-",
                @NameId = claims.Get(ClaimTypes.NameIdentifier),
                @Upn = claims.Get(ClaimTypes.Upn),
                @DisplayName = claims.Get("http://schemas.xmlsoap.org/claims/CommonName"),
                @AccessRequest=DateTime.Now,
                @IsSystem =0,
                @Locked=0,
                @Disabled=0,
                @Deleted=0,
                @CreatedOn=DateTime.Now,
                @CreatedBy="System",
                @ModifiedOn = DateTime.Now,
                @ModifiedBy = "System",
                @FirstName= claims.Get(ClaimTypes.GivenName),
                @LastName= claims.Get(ClaimTypes.Surname),
                @Gender=0,
                @Email1= claims.Get(ClaimTypes.Email),

            });

            var newUser = await FindByExternalProvider(provider, providerUserId, claims);
            return newUser;
        }



        private string _upSql = @"UPDATE [acl].[Users]
                                    SET [NameId] = @NameId,[Upn] = @Upn,[DisplayName] =@DspName,[AccessRequest] = @AcrDate
                                WHERE UserId =@UserId";
        public async Task<CustomUser> FindByExternalProvider(string provider, string providerUserId,List<Claim> claims)
        {
            using var connection = _context.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<CustomUser>($"{_sql} AND usr.UserId=@Id", new { Id = providerUserId });
            if (user is {AccessGranted: null})
            {
                var rs = await connection.ExecuteAsync(_upSql, new
                {
                    @UserId = providerUserId,
                    @NameId = claims.Get(ClaimTypes.NameIdentifier),
                    @Upn = claims.Get(ClaimTypes.Upn),
                    @DspName = claims.Get("http://schemas.xmlsoap.org/claims/CommonName"),
                    @AcrDate = DateTime.Now,
                    @ModifiedOn = DateTime.Now,
                    @ModifiedBy = "System",
                });

            }

            return user;
        }


        string _sql = @"SELECT usr.Id,usr.Email1 AS Email,usr.UserId,usr.PasswordHash,
                    usr.DisplayName AS DisplayName,usr.AccessRequest,usr.AccessGranted FROM acl.Users usr WHERE usr.Deleted=0 AND usr.Disabled=0";
        public async Task<CustomUser> FindBySubjectId(string subjectId)
        {
            if (!long.TryParse(subjectId, out var userId))
                throw new System.Exception($"unable to find user for {subjectId}");
            using var connection = _context.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<CustomUser>($"{_sql} AND usr.Id =@Id", new { Id = userId });
            return user;
        }

        public async Task<CustomUser> FindByUsername(string username)
        {
            using var connection = _context.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<CustomUser>($"{_sql} AND usr.UserId =@Id", new { Id = username });
            return user;
        }

        public async Task<List<Claim>> GetClaims(string subjectId)
        {
            var user = await FindBySubjectId(subjectId);
            if (user == null) throw new System.Exception($"Failed to get user");
            var claims = new List<Claim>
            {
                 new Claim("nickname", user.DisplayName),
                new Claim(JwtClaimTypes.Subject, $"{user.Id}"),
                  new Claim(ClaimTypes.Sid, $"{user.Id}"),
                new Claim(JwtClaimTypes.Id, user.UserId),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName),
            };

            //role claim
            // var values = new {userid=user.UserId};
            using var connection = _context.CreateConnection();
            var roles = await connection.QueryAsync<string>("[acl].[GetUserRoles]", new { userId = user.UserId }, commandType: System.Data.CommandType.StoredProcedure);
            if (roles.Any())
            {
                var rc = string.Join('|', roles.ToArray());
                claims.Add(new Claim(ClaimTypes.Role, rc));
                claims.Add(new Claim("rst", "0"));
            }
            else
            { claims.Add(new Claim("rst", "1")); }
            return claims;
        }

        public async Task<bool> ValidateCredentials(string username, string password)
        {
            var user = await FindByUsername(username);
            if (user != null)
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return false;
        }
    }
}
