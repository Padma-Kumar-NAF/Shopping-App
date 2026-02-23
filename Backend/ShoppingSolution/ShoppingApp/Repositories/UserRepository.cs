using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Stock;
using ShoppingApp.Models.DTOs.User;
using Microsoft.EntityFrameworkCore;

namespace ShoppingApp.Repositories
{
    public class UserRepository : Repository<Guid, User>, IUserRepository
    {
        //protected readonly IRepository<Guid, User> _repository;
        //public UserRepository(IRepository<Guid, User> repository)
        //{
        //    _repository = repository;
        //}

        public UserRepository(ShoppingContext context) : base(context)
        {

        }
        public async Task<User?> AddUser(User NewUser)
        {
            var user = await base.AddAsync(NewUser);
            return user;
        }

        public async Task<User?> GetUserByMail(string email)
        {
            return await base.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<IEnumerable<GetUsersResponseDTO>> GetUsers(int Limit, int PageNumber)
        {
            return await _context.Users
                .Include(u => u.UserDetails)
                .OrderBy(u => u.Name)
                .Skip((PageNumber - 1) * Limit)
                .Take(Limit)
                .Select (s => new GetUsersResponseDTO
                {
                    UserId = s.UserId,
                    UserDetailsId = s.UserDetails!.UserDetailsId,
                    Name = s.Name,
                    Email = s.Email,
                    Role = s.Role,
                    PhoneNumber = s.UserDetails.PhoneNumber,
                    AddressLine1 = s.UserDetails.AddressLine1,
                    AddressLine2 = s.UserDetails.AddressLine2,
                    State = s.UserDetails.State,
                    City = s.UserDetails.City,
                    Pincode = s.UserDetails.Pincode
                }).ToListAsync();
        }
    }
}
