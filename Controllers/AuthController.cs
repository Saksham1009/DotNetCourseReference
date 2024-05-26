using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controller
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
            _authHelper = new AuthHelper(configuration);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegDTO user)
        {
            if (user.Password != user.PasswordConfirm)
            {
                throw new Exception("Entered Passwords do not match");
            }

            string sql = "SELECT * FROM TutorialAppSchema.Auth WHERE Email = '" + user.Email + "';";
            IEnumerable<string> existingUser = _dapper.LoadData<string>(sql);
            if (existingUser.Count() > 0)
            {
                throw new Exception("The user with this email already exists");
            }

            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(user.Password, passwordSalt);

            string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth ([Email],
                                    [PasswordHash],
                                    [PasswordSalt]) VALUES (
                                        '" + user.Email + "', @PasswordHash, @PasswordSalt)";

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", System.Data.SqlDbType.VarBinary);
            passwordSaltParameter.Value = passwordSalt;

            SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", System.Data.SqlDbType.VarBinary);
            passwordHashParameter.Value = passwordHash;
            
            sqlParameters.Add(passwordHashParameter);
            sqlParameters.Add(passwordSaltParameter);

            if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
            {
                string sqlToAddUser = @"
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
                        user.Gender + "' , 1);";
                if (_dapper.ExecuteSql(sqlToAddUser))
                {
                    return Ok();
                }
                
                throw new Exception("Failed to register a user");
            }
            
            throw new Exception("Failed to register user");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO user)
        {
            string sqlForHashAndSalt = @"select
                                            [PasswordHash],
                                            [PasswordSalt] from TutorialAppSchema.Auth WHERE Email = '" + user.Email +"';";
            UserForLoginConfirmationDTO userForLoginConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDTO>(sqlForHashAndSalt);
            
            if (userForLoginConfirmation == null)
            {
                throw new Exception("There does not exist a user with this Email");
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(user.Password, userForLoginConfirmation.PasswordSalt);
            
            // if (userForLoginConfirmation.PasswordHash != passwordHash) // Won't work, will compare the pointers

            for (int i=0; i<passwordHash.Length; i++)
            {
                if (passwordHash[i] != userForLoginConfirmation.PasswordHash[i])
                {
                    return StatusCode(401, "Entered Password is incorrect");
                }
            }

            string sqlForUserId = @"SELECT [USERID] FROM TutorialAppSchema.Users WHERE Email = '" + user.Email + "';";

            int userId = _dapper.LoadDataSingle<int>(sqlForUserId);

            return Ok(new Dictionary<String, String> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            string userIdSql = @"SELECT UserId from TutorialAppSchema.Users WHERE UserId = " + userId + ";";

            int returnedUserId = _dapper.LoadDataSingle<int>(userIdSql);
            
            return Ok(new Dictionary<String, String> {
                {"token", _authHelper.CreateToken(returnedUserId)}
            });
        }
    }
}