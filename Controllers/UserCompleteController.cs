using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;

    public UserCompleteController(IConfiguration config)
    {
        // Console.WriteLine(config.GetConnectionString("DefaultConnection"));
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get
            @UserId = = @UserIdParameter";
        string stringParameter = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if (userId != 0)
        {
            stringParameter += ", @UserId = @UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
        if (isActive)
        {
            stringParameter += ", @Active = @ActiveParameter";
            sqlParameters.Add("@ActiveParameter", isActive, DbType.Boolean);
        }
        if (stringParameter.Length > 0)
        {
        sql += stringParameter.Substring(1);
        }

        IEnumerable<UserComplete> users = _dapper.LoadDataWithParam<UserComplete>(sql, sqlParameters);
        return users;
    }
    
    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        string sql = $@"EXEC TutorialAppSchema.spUser_Upsert
            @FirstName = @FirstNameParameter
            , @LastName = @LastNameParameter
            , @Email = @EmailParameter
            , @Gender = @GenderParameter
            , @JobTitle = @JobTitleParameter
            , @Department = @DepartmentParameter
            , @Salary = @SalaryParameter
            , @Active = @ActiveParameter
            , @UserId =  @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@FirstNameParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@LastNameParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@EmailParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@GenderParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@JobTitleParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@DepartmentParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@SalaryParameter", user.FirstName, DbType.Decimal);
            sqlParameters.Add("@ActiveParameter", user.FirstName, DbType.Boolean);
            sqlParameters.Add("@UserIdParameter", user.FirstName, DbType.Int32);

        if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to Execute");
    }
    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = $@"EXEC TutorialAppSchema.spUser_Delete 
            @UserId =  @UserIdParameter";

        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

        if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }
}
