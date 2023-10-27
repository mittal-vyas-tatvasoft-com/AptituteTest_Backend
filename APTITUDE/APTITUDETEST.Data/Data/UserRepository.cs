﻿using AptitudeTest.Common.Data;
using AptitudeTest.Core.Interfaces;
using AptitudeTest.Core.ViewModels;
using AptitudeTest.Data.Common;
using APTITUDETEST.Common.Data;
using AptitudeTest.Common.Helpers;
using APTITUDETEST.Core.Entities.Users;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text;


namespace AptitudeTest.Data.Data
{
    public class UserRepository : IUsersRepository
    {

        #region Properies
        private readonly AppDbContext _appDbContext;
        private readonly DapperAppDbContext _dapperContext;
        private readonly IConfiguration _config;
        private readonly string connectionString;

        #endregion

        #region Constructor
        public UserRepository(AppDbContext appDbContext, IConfiguration config, DapperAppDbContext dapperContext)
        {
            _appDbContext = appDbContext;
            _dapperContext = dapperContext;
            _config = config;
            connectionString = _config["ConnectionStrings:AptitudeTest"];
        }
        #endregion

        #region methods

        #region GetAllUsers
        public async Task<JsonResult> GetAllUsers(string? searchQuery, int? currentPageIndex, int? pageSize)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        List<UserViewModel> data = connection.Query<UserViewModel>("Select * from getAllUsers(@SearchQuery)", new { SearchQuery = searchQuery }).ToList();
                        PaginationVM<UserViewModel> paginatedData = Pagination<UserViewModel>.Paginate(data, pageSize, currentPageIndex);
                        connection.Close();
                        return new JsonResult(new ApiResponse<PaginationVM<UserViewModel>>
                        {
                            Data = paginatedData,
                            Message = ResponseMessages.Success,
                            Result = true,
                            StatusCode = ResponseStatusCode.Success
                        });
                    }
                }
                else
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        List<UserViewModel> data = connection.Query<UserViewModel>("Select * from getAllUsers(@SearchQuery)", new { SearchQuery = "" }).ToList();
                        PaginationVM<UserViewModel> paginatedData = Pagination<UserViewModel>.Paginate(data, pageSize, currentPageIndex);
                        connection.Close();
                        return new JsonResult(new ApiResponse<PaginationVM<UserViewModel>>
                        {
                            Data = paginatedData,
                            Message = ResponseMessages.Success,
                            Result = true,
                            StatusCode = ResponseStatusCode.Success
                        });
                    }


                }
            }
            catch
            {
                return new JsonResult(new ApiResponse<List<UserViewModel>>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
                });
            }
        }

        #endregion

        #region GetUserById
        public async Task<JsonResult> GetUserById(int id)
        {
            try
            {
                UserDetailsVM userDetails = new UserDetailsVM();
                using (var dbConnection = new DbConnection())
                {
                    var data = dbConnection.Connection.Query("Select * from GetUserbyId(@user_id)", new { user_id = id }).ToList();

                    if (data.Count == 0)
                    {
                        return new JsonResult(new ApiResponse<UserDetailsVM>
                        {
                            Data = null,
                            Message = string.Format(ResponseMessages.NotFound, "User"),
                            Result = false,
                            StatusCode = ResponseStatusCode.NotFound
                        });
                    }
                    userDetails.AcademicsDetails = new List<UserAcademicsVM>();
                    userDetails.FamilyDetails = new List<UserFamilyVM>();
                    List<int> FamilyIds = new List<int>();
                    List<int> AcadamicIds = new List<int>();
                    FillUserData(userDetails, data);
                    FillAcademicAndFamilyData(data, userDetails, AcadamicIds, FamilyIds);

                }

                return new JsonResult(new ApiResponse<UserDetailsVM>
                {
                    Data = userDetails,
                    Message = ResponseMessages.Success,
                    Result = true,
                    StatusCode = ResponseStatusCode.Success
                });
            }

            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
                });
            }
        }
        #endregion

        #region Create
        public async Task<JsonResult> Create(CreateUserVM user)
        {
            var pass = RandomPasswordGenerator.GenerateRandomPassword(8);
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var procedure = "insert_user";
                    var parameters = new DynamicParameters(
                        new
                        {
                            p_firstname = user.FirstName,
                            p_lastname = user.LastName,
                            p_fathername = user.FatherName,
                            p_email = user.Email,
                            p_password = pass,
                            p_phonenumber = user.PhoneNumber,
                            p_groupid = user.GroupId,
                            p_collegeid = user.CollegeId,
                            p_gender = user.Gender,
                            p_status = user.Status,
                            p_createdby = user.CreatedBy,
                            next_id = 0
                        });

                    var userId = connection.Query<int>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if(userId > 0)
                    { 
                        return new JsonResult(new ApiResponse<string>
                        {
                            Message = string.Format(ResponseMessages.AddSuccess, "User"),
                            Result = true,
                            StatusCode = ResponseStatusCode.Success
                        }); ;
                    }
                    else
                    {
                        return new JsonResult(new ApiResponse<string>
                        {
                            Message = string.Format(ResponseMessages.InternalError, "User"),
                            Result = false,
                            StatusCode = ResponseStatusCode.RequestFailed
                        }); ;
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.InternalError, "User"),
                    Result = false,
                    StatusCode = ResponseStatusCode.RequestFailed
                });
            }
        }
        #endregion

        #region Update
        public async Task<JsonResult> Update(UserVM user)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var procedure = "udpate_user_transaction";
                    var dateParameter = new NpgsqlParameter("p_dateofbirth", NpgsqlDbType.Date);
                    dateParameter.Value = user.DateOfBirth;
                    var parameters = new DynamicParameters(
                    new
                    {
                        p_userid = user.Id,
                        p_groupid = user.GroupId,
                        p_collegeid = user.CollegeId,
                        p_status = user.Status,
                        p_firstname = user.FirstName,
                        p_lastname = user.LastName,
                        p_fathername = user.FatherName,
                        p_gender = user.Gender,
                        p_dateofbirth = dateParameter.Value,
                        p_email = user.Email,
                        p_phonenumber = user.PhoneNumber,
                        p_level = user.Level,
                        p_appliedthrough = user.AppliedThrough,
                        p_technologyinterestedin = user.TechnologyInterestedIn,
                        p_permanentaddress1 = user.PermanentAddress1,
                        p_permanentaddress2 = user.PermanentAddress2,
                        p_pincode = user.Pincode,
                        p_cityid = user.CityId,
                        p_stateid= user.StateId,                       
                        p_acpcmeritrank = user.ACPCMeritRank,
                        p_gujcetscore = user.GUJCETScore,
                        p_jeescore = user.JEEScore,
                        p_updatedby = user.UpdatedBy,
                        p_userfamilydata = user.UserFamilyVM.ToArray(),
                        p_useracademicsdata = user.UserAcademicsVM.ToArray()
                    }); ;

                    connection.Query(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.AddSuccess, "User"),
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.InternalError, "User"),
                    Result = false,
                    StatusCode = ResponseStatusCode.RequestFailed
                });
            }
        }


        #endregion

        #region InActiveUsers
        public async Task<JsonResult> ActiveInActiveUsers(UserStatusVM userStatusVM)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var procedure = "activeinactive_users";
                    var parameters = new DynamicParameters(
                    new
                    {
                        p_userids = userStatusVM.UserIds.ToArray(),
                        p_status = userStatusVM.status
                    });

                    connection.Query(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.Success),
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.InternalError, "User"),
                    Result = false,
                    StatusCode = ResponseStatusCode.RequestFailed
                });
            }
        }
        #endregion

        #region DeleteUsers
        public async Task<JsonResult> DeleteUsers(List<int> userIds)
        {
            try
            {
                using (var connection = _dapperContext.CreateConnection())
                {
                    var procedure = "delete_users";
                    var parameters = new DynamicParameters(
                    new
                    {
                        p_userids = userIds.ToArray()
                    });

                    connection.Query(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.Success),
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.InternalError, "User"),
                    Result = false,
                    StatusCode = ResponseStatusCode.RequestFailed
                });
            }
        }
        #endregion

        #region HelpingMethods

        private void FillUserData(UserDetailsVM userDetails, dynamic data)
        {
            var userData = data[0];
            userDetails.UserId = userData.userid ?? 0;
            userDetails.FirstName = userData.firstname ?? "";
            userDetails.LastName = userData.lastname ?? "";
            userDetails.Email = userData.email ?? "";
            userDetails.PhoneNumber = userData.phonenumber ?? 0;
            userDetails.FatherName = userData.fathername ?? "";
            userDetails.Level = userData.level ?? 0;
            userDetails.DateOfBirth = userData.dateofbirth ?? new DateTime();
            userDetails.PermanentAddress = userData.permanentaddress ?? "";
            userDetails.UserGroup = userData.usergroup ?? 0;
            userDetails.GroupName = userData.groupname ?? "";
            userDetails.AppliedThrough = userData.appliedthrough ?? 0;
            userDetails.TechnologyInterestedIn = userData.technologyinterestedIn ?? 0;
            userDetails.TechnologyName = userData.technologyname ?? "";
            userDetails.ACPCMeritRank = userData.acpcmeritrank ?? 0;
            userDetails.GUJCETScore = userData.gujcetscore ?? 0;
            userDetails.JEEScore = userData.jeescore ?? 0;
            userDetails.Gender = userData.gender ?? 0;
            userDetails.PreferedLocation = userData.preferedlocation ?? 0;
            userDetails.RelationshipWithExistingEmployee = userData.relationshipwithexistingemployee ?? "";
            userDetails.Status = userData.status ?? 0;
            userDetails.RoleId = userData.roleid ?? 0;
        }

        private void FillAcademicAndFamilyData(dynamic data, UserDetailsVM userDetails, List<int> acadamicIds, List<int> familyIds)
        {
            foreach (var entity in data)
            {
                if (entity.AcademicId != null)
                {
                    if (!acadamicIds.Contains(entity.AcademicId))
                    {
                        UserAcademicsVM userAcademics = new UserAcademicsVM();
                        userAcademics.AcademicId = entity.AcademicId;
                        userAcademics.DegreeId = entity.DegreeId;
                        userAcademics.StreamId = entity.StreamId;
                        userAcademics.Physics = entity.Physics;
                        userAcademics.Maths = entity.Maths;
                        userAcademics.Grade = entity.Grade;
                        userAcademics.University = entity.University;
                        userAcademics.DurationFromYear = entity.DurationFromYear;
                        userAcademics.DurationFromMonth = entity.DurationFromMonth;
                        userAcademics.DurationToYear = entity.DurationToYear;
                        userAcademics.DurationToMonth = entity.DurationToMonth;
                        userAcademics.DegreeName = entity.degreename;
                        userAcademics.DegreeLevel = entity.degreelevel;
                        userAcademics.StreamName = entity.streamname;
                        userDetails.AcademicsDetails.Add(userAcademics);
                    }
                    acadamicIds.Add(entity.AcademicId);
                }

                if (entity.familyid != null)
                {
                    if (!familyIds.Contains(entity.familyid))
                    {
                        UserFamilyVM family = new UserFamilyVM();
                        family.FamilyId = entity.familyid;
                        family.FamilyPerson = entity.FamilyPerson;
                        family.Qualification = entity.Qualification;
                        family.Occupation = entity.Occupation;
                        userDetails.FamilyDetails.Add(family);
                    }
                    familyIds.Add(entity.familyid);
                }
            }
        }
       


       


    #endregion

    #endregion

}
}
