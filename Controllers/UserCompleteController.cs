using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.DTOs;

namespace DotnetAPI.Controller;

[ApiController] // helps recieve and send data in correct format
[Route("[controller]")] // assigns the name before the Controller keyword in this case WeatherForecast and assigns it to the endpoint
public class UserCompleteController : ControllerBase
{

    DataContextDapper _dapper;

    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }

    [HttpGet("testconnection")]
    public String TestConnection()
    {
        return _dapper.LoadDataSingle<String>("SELECT GETDATE()");
    }

    [HttpGet("test/{inputValue}")]
    // public IActionResult Test()
    public string[] Test(string inputValue)
    {
        string[] resultOutput = new string[] {
            "Test 1",
            "Test 2",
            "Test 3",
            "Test 4 Saksham lol ",
            inputValue
        };
        return resultOutput;
    }

    [HttpGet("GetUsers")]
    public IEnumerable<UserComplete> GetUsers()
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }

    [HttpGet("GetUserByName/{firstName}")]
    public User GetUserByName(string firstName)
    {
        string sql = @"
        select [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        from TutorialAppSchema.Users
        WHERE FirstName = '" + firstName + "';";

        User user = _dapper.LoadDataSingle<User>(sql);
        if (user != null)
        {
            return user;
        }

        throw new Exception("Could not find a user with the first name " + firstName);
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql = @"
        select [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        from TutorialAppSchema.Users
        WHERE UserId = " + userId.ToString() + ";";
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] = '" + user.FirstName + "'," +
                " [LastName] = '" + user.LastName + "'," +
                " [Email] = '" + user.Email + "'," + 
                " [Gender] = '" + user.Gender + "'," +
                " [Active] = '" + user.Active + "'" +
            " WHERE UserId = " + user.UserId + ";";
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Editing user was not successful");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDTO user)
    {
        string sql = @"
            INSERT into TutorialAppSchema.Users(
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
            ) VALUES ( '" +
                user.FirstName + "', '" +
                user.LastName + "', '" +
                user.Email + "' , '" +
                user.Gender + "' , '" +
                user.Active + "' );";
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Adding user was not successful");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
        DELETE FROM TutorialAppSchema.Users
            WHERE UserId = " + userId.ToString() + ";";

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user");
    }
}