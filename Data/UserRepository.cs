using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        DataContextEF _entity;

        public UserRepository(IConfiguration configuration)
        {
            _entity = new DataContextEF(configuration);
        }

        public bool SaveChanges()
        {
            return _entity.SaveChanges() > 0;
        }

        public int SaveChangesWithRowCount()
        {
            return _entity.SaveChanges();
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entity.Add(entityToAdd);
            }
        }
        
        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entity.Remove(entityToRemove);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entity.Users.ToList<User>();
            return users;
        }

        public User GetUserByName(string firstName)
        {
            User? user = _entity.Users
                .Where(user => user.FirstName == firstName)
                .FirstOrDefault<User>();

            if (user != null)
            {
                return user;
            }

            throw new Exception("Could not find a user with the first name " + firstName);
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entity.Users
                .Where(user => user.UserId == userId)
                .FirstOrDefault<User>();

            if (user != null)
            {
                return user;
            }

            throw new Exception("Could not find a user");
        }
    }
}