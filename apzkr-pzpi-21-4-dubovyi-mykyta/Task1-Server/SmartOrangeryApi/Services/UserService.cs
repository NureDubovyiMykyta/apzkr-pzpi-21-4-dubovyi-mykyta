using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartOrangeryApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartOrangeryApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly JwtSettings _jwtSettings;

        public UserService(MongoDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _users = context.Users;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<List<User>> GetAsync() =>
            await _users.Find(user => !user.IsDeleted).ToListAsync();

        public async Task<User> GetAsync(string id) =>
            await _users.Find<User>(user => user.Id == new ObjectId(id) && !user.IsDeleted).FirstOrDefaultAsync();

        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find<User>(user => user.Email == email && !user.IsDeleted).FirstOrDefaultAsync();

        public async Task CreateAsync(User user)
        {
            user.Id = ObjectId.GenerateNewId();
            user.CreatedById = user.Id;
            user.CreatedDateUtc = DateTime.UtcNow;
            user.IsDeleted = false;

            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(string id, User updatedUser, ObjectId userId)
        {
            var existingUser = await _users.Find(user => user.Id == new ObjectId(id) && !user.IsDeleted).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            updatedUser.Id = existingUser.Id;
            updatedUser.CreatedById = existingUser.CreatedById;
            updatedUser.CreatedDateUtc = existingUser.CreatedDateUtc;
            updatedUser.IsDeleted = existingUser.IsDeleted;
            updatedUser.LastModifiedById = userId;
            updatedUser.LastModifiedDateUtc = DateTime.UtcNow;

            await _users.ReplaceOneAsync(user => user.Id == new ObjectId(id), updatedUser);
        }

        public async Task RemoveAsync(string id, ObjectId userId)
        {
            var update = Builders<User>.Update
                .Set(user => user.IsDeleted, true)
                .Set(user => user.LastModifiedById, userId)
                .Set(user => user.LastModifiedDateUtc, DateTime.UtcNow);

            await _users.UpdateOneAsync(user => user.Id == new ObjectId(id), update);
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
