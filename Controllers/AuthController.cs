using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace Dotnet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration){
            if(userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExist = $@"SELECT Email FROM TutorialAppSchema.Auth
                    WHERE Email = '{userForRegistration.Email}'";

                IEnumerable<string> existUser = _dapper.LoadData<string>(sqlCheckUserExist);
                
                if(existUser.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto() {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };
                    

                    if(_authHelper.SetPassword(userForSetPassword))
                    {
                        string sqlAddUser = $@"EXEC TutorialAppSchema.spUser_Upsert
                            @FirstName = '{userForRegistration.FirstName}'
                            , @LastName = '{userForRegistration.LastName}'
                            , @Email = '{userForRegistration.Email}'
                            , @Gender = '{userForRegistration.Gender}'
                            , @JobTitle = '{userForRegistration.JobTitle}'
                            , @Department = '{userForRegistration.Department}'
                            , @Salary = '{userForRegistration.Salary}'
                            , @Active = 1";
                        
                        if(_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user");
                    }
                    throw new Exception("Failed to register User");
                }
                throw new Exception("User with this Email already exists");
            }
            throw new Exception("Password Do not match");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword){
            if(_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = $@"EXEC TutorialAppSchema.spLoginConfirmation_Get 
                @Email = @EmailParam";

                DynamicParameters sqlParameters = new DynamicParameters();

                    // SqlParameter emailParam = new SqlParameter("@emailParam",SqlDbType.VarChar);
                    // emailParam.Value = userForLogin.Email;
                    // sqlParameters.Add(emailParam);
                    sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingleWithParam<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            // if(passwordHash == userForConfirmation.PasswordHash) Won't work because they are objects
            for(int i=0; i < passwordHash.Length; i++)
            {
                if(passwordHash[i] != userForConfirmation.PasswordHash[i])
                {
                    return StatusCode(401,"Incorrect Password");
                }
            }

            string UserIdSql = $@"SELECT UserId FROM TutorialAppSchema.Users 
                                    WHERE Email = '{userForLogin.Email}'";

            int userId = _dapper.LoadDataSingle<int>(UserIdSql);


            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }
        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string UserIdSql = $@"SELECT UserId FROM TutorialAppSchema.Users 
                                    WHERE UserId = '{User.FindFirstValue("UserId")}'";
            
            int userId = _dapper.LoadDataSingle<int>(UserIdSql);

            return _authHelper.CreateToken(userId);
        }
    }
}