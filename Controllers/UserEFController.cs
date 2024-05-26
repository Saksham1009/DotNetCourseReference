using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.DTOs;

namespace DotnetAPI.Controller;

[ApiController] // helps recieve and send data in correct format
[Route("[controller]")] // assigns the name before the Controller keyword in this case WeatherForecast and assigns it to the endpoint
public class UserEFController : ControllerBase
{
    IUserRepository _userRepository;

    public UserEFController(IConfiguration configuration, IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetUserByName/{firstName}")]
    public User GetUserByName(string firstName)
    {
        return _userRepository.GetUserByName(firstName);
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDB = _userRepository.GetSingleUser(user.UserId);

        if (userDB != null)
        {
            userDB.FirstName = user.FirstName;
            userDB.LastName = user.LastName;
            userDB.Email = user.Email;
            userDB.Active = user.Active;
            userDB.Gender = user.Gender;
        
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }

        throw new Exception("Editing user was not successful");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDTO user)
    {
        User userDB = new User();

        userDB.FirstName = user.FirstName;
        userDB.LastName = user.LastName;
        userDB.Email = user.Email;
        userDB.Active = user.Active;
        userDB.Gender = user.Gender;

        _userRepository.AddEntity<User>(userDB);
        
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Adding user was not successful");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDB = _userRepository.GetSingleUser(userId);

        if (userDB != null)
        {
            _userRepository.RemoveEntity<User>(userDB);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }

        throw new Exception("Failed to delete user");
    }
}