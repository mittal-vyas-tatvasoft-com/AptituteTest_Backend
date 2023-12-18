﻿using AptitudeTest.Core.Interfaces;
using AptitudeTest.Core.ViewModels;
using AptitudeTest.Data.Common;
using APTITUDETEST.Common.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AptitudeTest.Data.Data
{
    public class ResultRepository : IResultRepository
    {
        #region Properties
        private readonly string? connectionString;
        #endregion

        #region Constructor
        public ResultRepository(AppDbContext context, IConfiguration config)
        {
            IConfiguration _config;
            _config = config;
            connectionString = _config["ConnectionStrings:AptitudeTest"];
        }
        #endregion

        #region Methods
        public async Task<JsonResult> Get(int userId, int testId, int marks, int pageSize, int pageIndex)
        {
            try
            {
                if (userId < 1)
                {
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = ResponseMessages.BadRequest,
                        Result = false,
                        StatusCode = ResponseStatusCode.BadRequest
                    });
                }

                using (DbConnection connection = new DbConnection())
                {
                    var data = await connection.Connection.QueryAsync<UserTestResultDataVM>("select * from getUserResults(@userId,@testId)", new { userId = userId, testId = testId });
                    if (!data.Any())
                    {
                        return new JsonResult(new ApiResponse<UserDetailsVM>
                        {
                            Data = null,
                            Message = string.Format(ResponseMessages.NotFound, ModuleNames.Question),
                            Result = false,
                            StatusCode = ResponseStatusCode.NotFound
                        });
                    }

                    string userName = "";
                    List<UserResultQuestionVM> userResultQuestionVMList;

                    userResultQuestionVMList = data.GroupBy(q => q.QuestionId).Select(x =>
                    {
                        var temp = x.FirstOrDefault();
                        userName = temp.Name;
                        return new UserResultQuestionVM()
                        {
                            Difficulty = temp.Difficulty,
                            Id = temp.QuestionId,
                            OptionType = temp.OptionType,
                            QuestionText = temp.QuestionText,
                            Options = x.Select(k =>
                            {
                                return new UserResultOptionVM() { OptionId = k.OptionId, IsAnswer = k.IsAnswer, OptionValue = k.OptionData, IsUserAnswer = k.UserAnswers != null ? k.UserAnswers.Contains(k.OptionId) : false };

                            }).ToList(),
                            UserAnswers = temp.UserAnswers == null ? null : temp.UserAnswers.ToList()

                        };
                    }).ToList();
                    UserResultsVM tempData = getQuestionCount(userResultQuestionVMList);
                    if (marks != 0)
                    {
                        userResultQuestionVMList = userResultQuestionVMList.Where(x => x.Difficulty == marks).ToList();
                    }

                    PaginationVM<UserResultQuestionVM> paginatedData = new PaginationVM<UserResultQuestionVM>()
                    {
                        EntityList = userResultQuestionVMList.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                    };

                    UserResultsVM userResultsVM = new UserResultsVM()
                    {
                        AllCorrectQuestionCount = tempData.AllCorrectQuestionCount,
                        AllQuestionCount = tempData.AllQuestionCount,
                        Marks5CorrectQuestionCount = tempData.Marks5CorrectQuestionCount,
                        Marks1CorrectQuestionCount = tempData.Marks1CorrectQuestionCount,
                        Marks1QuestionCount = tempData.Marks1QuestionCount,
                        Marks2CorrectQuestionCount = tempData.Marks2CorrectQuestionCount,
                        Marks2QuestionCount = tempData.Marks2QuestionCount,
                        Marks3CorrectQuestionCount = tempData.Marks3CorrectQuestionCount,
                        Marks3QuestionCount = tempData.Marks3QuestionCount,
                        Marks4CorrectQuestionCount = tempData.Marks4CorrectQuestionCount,
                        Marks4QuestionCount = tempData.Marks4QuestionCount,
                        Marks5QuestionCount = tempData.Marks5QuestionCount,
                        Name = userName,
                        UserId = userId,
                        PaginatedData = paginatedData
                    };

                    return new JsonResult(new ApiResponse<UserResultsVM>
                    {
                        Data = userResultsVM,
                        Message = ResponseMessages.Success,
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }

            catch
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
                });
            }
        }

        public async Task<JsonResult> GetResults(string? searchQuery, int? TestId, int? GroupId, int? CollegeId, int? Year, int? currentPageIndex, int? pageSize, string? sortField, string? sortOrder)
        {
            try
            {
                searchQuery = string.IsNullOrEmpty(searchQuery) ? string.Empty : searchQuery;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    List<ResultsVM> data = connection.Query<ResultsVM>("Select * from getallresults_3(@SearchQuery,@GroupId,@CollegeId,@TestId,@YearAttended,@PageNumber,@PageSize,@SortField,@SortOrder)", new { SearchQuery = searchQuery, GroupId = (object)GroupId!, CollegeId = (object)CollegeId!, TestId = (object)TestId!, YearAttended = Year, PageNumber = currentPageIndex, PageSize = pageSize, SortField = sortField, SortOrder = sortOrder }).ToList();
                    return new JsonResult(new ApiResponse<List<ResultsVM>>
                    {
                        Data = data,
                        Message = ResponseMessages.Success,
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }

            }

            catch
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
                });
            }

        }

        public async Task<JsonResult> GetResultStatistics(string? searchQuery, int? TestId, int? GroupId, int? CollegeId, int? Year, int? currentPageIndex, string? sortField, string? sortOrder)
        {
            try
            {
                searchQuery = string.IsNullOrEmpty(searchQuery) ? string.Empty : searchQuery;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    List<ResultStatisticsVM> data = connection.Query<ResultStatisticsVM>("Select * from getstatisticsresult(@SearchQuery,@GroupId,@CollegeId,@TestId,@YearAttended,@PageNumber,@SortField,@SortOrder)", new { SearchQuery = searchQuery, GroupId = (object)GroupId!, CollegeId = (object)CollegeId!, TestId = (object)TestId!, YearAttended = Year, PageNumber = currentPageIndex, SortField = sortField, SortOrder = sortOrder }).ToList();
                    return new JsonResult(new ApiResponse<List<ResultStatisticsVM>>
                    {
                        Data = data,
                        Message = ResponseMessages.Success,
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }
            catch
            {
                return new JsonResult(new ApiResponse<string>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
                });
            }

        }

        public async Task<JsonResult> GetResultExportData(string? searchQuery, int? TestId, int? GroupId, int? CollegeId, int? Year, int? currentPageIndex, int? pageSize, string? sortField, string? sortOrder)
        {
            try
            {
                searchQuery = string.IsNullOrEmpty(searchQuery) ? string.Empty : searchQuery;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    List<ResultExportDataVM> data = connection.Query<ResultExportDataVM>("Select * from getallresultsexport(@SearchQuery,@GroupId,@CollegeId,@TestId,@YearAttended,@PageNumber,@PageSize,@SortField,@SortOrder)", new { SearchQuery = searchQuery, GroupId = (object)GroupId!, CollegeId = (object)CollegeId!, TestId = (object)TestId!, YearAttended = Year, PageSize = pageSize, PageNumber = currentPageIndex, SortField = sortField, SortOrder = sortOrder }).ToList();
                    return new JsonResult(new ApiResponse<List<ResultExportDataVM>>
                    {
                        Data = data,
                        Message = ResponseMessages.Success,
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
            }
            catch
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

        #region Helper Methods

        private UserResultsVM getQuestionCount(List<UserResultQuestionVM> data)
        {
            int totalCount = 0;
            int totalCorrectCount = 0;
            int total1MarkCorrectCount = 0;
            int total2MarkCorrectCount = 0;
            int total3MarkCorrectCount = 0;
            int total4MarkCorrectCount = 0;
            int total5MarkCorrectCount = 0;
            foreach (var item in data)
            {
                totalCount++;
                List<int> correctAnswers = item.Options.Where(o => o.IsAnswer).Select(o => o.OptionId).ToList();
                if (CompareList(item.UserAnswers, correctAnswers))
                {
                    totalCorrectCount++;
                    switch (item.Difficulty)
                    {
                        case 1:
                            total1MarkCorrectCount++;
                            break;
                        case 2:
                            total2MarkCorrectCount++;
                            break;
                        case 3:
                            total3MarkCorrectCount++;
                            break;
                        case 4:
                            total4MarkCorrectCount++;
                            break;
                        case 5:
                            total5MarkCorrectCount++;
                            break;
                    }
                }
            }

            int total1MarkCount = data.Count(x => x.Difficulty == 1);
            int total2MarkCount = data.Count(x => x.Difficulty == 2);
            int total3MarkCount = data.Count(x => x.Difficulty == 3);
            int total4MarkCount = data.Count(x => x.Difficulty == 4);
            int total5MarkCount = data.Count(x => x.Difficulty == 5);

            return new UserResultsVM()
            {
                AllCorrectQuestionCount = totalCorrectCount,
                AllQuestionCount = totalCount,

                Marks1QuestionCount = total1MarkCount,
                Marks2QuestionCount = total2MarkCount,
                Marks3QuestionCount = total3MarkCount,
                Marks4QuestionCount = total4MarkCount,
                Marks5QuestionCount = total5MarkCount,
                Marks1CorrectQuestionCount = total1MarkCorrectCount,
                Marks2CorrectQuestionCount = total2MarkCorrectCount,
                Marks3CorrectQuestionCount = total3MarkCorrectCount,
                Marks4CorrectQuestionCount = total4MarkCorrectCount,
                Marks5CorrectQuestionCount = total5MarkCorrectCount
            };



        }

        private bool CompareList(List<int> list1, List<int> list2)
        {
            if ((list1 == null && list2 != null) || (list1 != null && list2 == null))
            {
                return false;
            }

            if (list1 == null && list2 == null)
            {
                return true;
            }

            bool result = (list1.Count == list2.Count);
            if (result)
            {
                foreach (var item in list1)
                {
                    if (!list2.Contains(item))
                    {
                        return false;
                    }
                }

                foreach (var item in list2)
                {
                    if (!list1.Contains(item))
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
