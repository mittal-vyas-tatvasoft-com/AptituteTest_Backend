﻿using AptitudeTest.Core.Entities.Test;
using AptitudeTest.Core.Interfaces;
using AptitudeTest.Core.ViewModels;
using AptitudeTest.Data.Common;
using APTITUDETEST.Common.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AptitudeTest.Data.Data
{
    public class TestRepository : ITestRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string connectionString;

        public TestRepository(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            connectionString = _config["ConnectionStrings:AptitudeTest"];
        }

        #region Methods
        public async Task<JsonResult> GetTests(string? searchQuery, int? GroupId, int? Status, DateTime? Date, int? currentPageIndex, int? pageSize)
        {
            try
            {
                searchQuery = string.IsNullOrEmpty(searchQuery) ? string.Empty : searchQuery;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    List<TestsViewModel> data = connection.Query<TestsViewModel>("Select * from getalltests(@SearchQuery,@GroupId,@Status,@DateFilter,@PageNumber,@PageSize)", new { SearchQuery = searchQuery, GroupId = (object)GroupId, Status = Status, DateFilter = Date, PageNumber = currentPageIndex, PageSize = pageSize }).ToList();
                    connection.Close();
                    return new JsonResult(new ApiResponse<List<TestsViewModel>>
                    {
                        Data = data.OrderByDescending(x => x.Testid).ToList(),
                        Message = ResponseMessages.Success,
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }

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


        public async Task<JsonResult> CreateTest(CreateTestVM test)
        {
            try
            {
                Test? testAlreadyExists = _context.Tests.Where(t => t.Name == test.Name && t.Status == (int)Common.Enums.TestStatus.Active && t.IsDeleted == false).FirstOrDefault();
                if (testAlreadyExists == null)
                {
                    Test testToBeAdded = new Test()
                    {
                        Name = test.Name,
                        Description = test.Description,
                        Date = test.Date,
                        StartTime = test.StartTime,
                        EndTime = test.EndTime,
                        TestDuration = test.TestDuration,
                        Status = test.Status,
                        BasicPoint = test.BasicPoint,
                        MessaageAtStartOfTheTest = test.MessaageAtStartOfTheTest,
                        MessaageAtEndOfTheTest = test.MessaageAtEndOfTheTest,
                        IsRandomQuestion = test.IsRandomQuestion,
                        IsRandomAnswer = test.IsRandomAnswer,
                        IsLogoutWhenTimeExpire = test.IsLogoutWhenTimeExpire,
                        IsQuestionsMenu = test.IsQuestionsMenu,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = test.CreatedBy,
                    };

                    _context.Add(testToBeAdded);
                    _context.SaveChanges();
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.AddSuccess, ModuleNames.Test),
                        Result = true,
                        StatusCode = ResponseStatusCode.OK
                    });
                }
                else
                {
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.AlreadyExists, ModuleNames.Test),
                        Result = false,
                        StatusCode = ResponseStatusCode.AlreadyExist
                    });
                }

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

        public async Task<JsonResult> UpdateTestGroup(UpdateTestGroupVM updateTest)
        {
            try
            {
                Test testAlreadyExists = _context.Tests.Where(t => t.Id != updateTest.TestId && t.GroupId == updateTest.GroupId && t.Status == (int)Common.Enums.TestStatus.Active && t.IsDeleted == false).FirstOrDefault();
                if (testAlreadyExists != null)
                {
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.AlreadyExists, ModuleNames.Test),
                        Result = true,
                        StatusCode = ResponseStatusCode.OK
                    });
                }
                Test test = await Task.FromResult(_context.Tests.Where(t => t.Id == updateTest.TestId && t.Status == (int)Common.Enums.TestStatus.Active && t.IsDeleted == false).FirstOrDefault());
                if (test != null)
                {
                    test.GroupId = updateTest.GroupId;
                    test.UpdatedBy = updateTest.UpdatedBy;
                    test.UpdatedDate = DateTime.UtcNow;
                    _context.SaveChanges();

                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.UpdateSuccess, ModuleNames.Test),
                        Result = true,
                        StatusCode = ResponseStatusCode.Success
                    });
                }
                return new JsonResult(new ApiResponse<string>
                {
                    Message = ResponseMessages.InternalError,
                    Result = false,
                    StatusCode = ResponseStatusCode.InternalServerError
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

        public async Task<JsonResult> AddTestQuestions(AddTestQuestionsVM addTestQuestion)
        {
            try
            {
                Test test = await Task.FromResult(_context.Tests.Where(t => t.Id == addTestQuestion.TestId && t.Status == (int)Common.Enums.TestStatus.Active && t.IsDeleted == false).FirstOrDefault());
                if (test == null)
                {
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.NotFound, ModuleNames.Test),
                        Result = true,
                        StatusCode = ResponseStatusCode.OK
                    });
                }

                #region Code to check questions are available in DB
                Func<TestQuestionsCountVM, int> func = x => x.OneMarkQuestion;
                var MarkQuestionCountReq = 0;
                for (int questionType = 1; questionType <= 2; questionType++)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        int MarkQuestionCountInDB = await Task.FromResult(_context.Questions.Where(t => t.Topic == addTestQuestion.TopicId && t.QuestionType == questionType && t.Difficulty == i && t.IsDeleted == false).Count());
                        switch (i)
                        {
                            case 1:
                                func = x => x.OneMarkQuestion;
                                break;
                            case 2:
                                func = x => x.TwoMarkQuestion;
                                break;
                            case 3:
                                func = x => x.ThreeMarkQuestion;
                                break;
                            case 4:
                                func = x => x.FourMarkQuestion;
                                break;
                            case 5:
                                func = x => x.FiveMarkQuestion;
                                break;
                            default:
                                break;
                        }

                        MarkQuestionCountReq = addTestQuestion.TestQuestionsCount.Where(t => t.QuestionType == questionType).Select(func).FirstOrDefault();

                        if (MarkQuestionCountReq > MarkQuestionCountInDB)
                        {
                            return new JsonResult(new ApiResponse<string>
                            {
                                Message = string.Format(ResponseMessages.NotEnoughQuestion, i),
                                Result = true,
                                StatusCode = ResponseStatusCode.OK
                            });
                        }
                    }
                }
                #endregion
                
                TestQuestions testQuestion = await Task.FromResult(_context.TestQuestions.Where(t => t.TestId == addTestQuestion.TestId && t.TopicId == addTestQuestion.TopicId && t.IsDeleted == false).FirstOrDefault());
                if (testQuestion != null)
                {
                    return new JsonResult(new ApiResponse<string>
                    {
                        Message = string.Format(ResponseMessages.TestTopicAlreadyExists),
                        Result = true,
                        StatusCode = ResponseStatusCode.OK
                    });
                }

                TestQuestions testQuestionsToBeAdded = new TestQuestions();

                testQuestionsToBeAdded.TestId = addTestQuestion.TestId;
                testQuestionsToBeAdded.TopicId = addTestQuestion.TopicId;
                testQuestionsToBeAdded.NoOfQuestions = addTestQuestion.NoOfQuestions;
                testQuestionsToBeAdded.Weightage = addTestQuestion.Weightage;
                testQuestionsToBeAdded.CreatedDate = DateTime.UtcNow;
                testQuestionsToBeAdded.CreatedBy = test.CreatedBy;

                _context.Add(testQuestionsToBeAdded);
                _context.SaveChanges();

                TestQuestionsCount testQuestionsCountToBeAdded = new TestQuestionsCount();

                foreach (var testQuestionCount in addTestQuestion.TestQuestionsCount)
                {
                    testQuestionsCountToBeAdded.TestQuestionId = testQuestionsToBeAdded.Id;
                    testQuestionsCountToBeAdded.QuestionType = testQuestionCount.QuestionType;
                    testQuestionsCountToBeAdded.OneMarks = testQuestionCount.OneMarkQuestion;
                    testQuestionsCountToBeAdded.TwoMarks = testQuestionCount.TwoMarkQuestion;
                    testQuestionsCountToBeAdded.ThreeMarks = testQuestionCount.ThreeMarkQuestion;
                    testQuestionsCountToBeAdded.FourMarks = testQuestionCount.FourMarkQuestion;
                    testQuestionsCountToBeAdded.FiveMarks = testQuestionCount.FiveMarkQuestion;
                    testQuestionsCountToBeAdded.CreatedDate = DateTime.UtcNow;
                    testQuestionsCountToBeAdded.CreatedBy = test.CreatedBy;

                    _context.Add(testQuestionsCountToBeAdded);
                    _context.SaveChanges();
                }

                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.AddSuccess, ModuleNames.TestQuestions),
                    Result = true,
                    StatusCode = ResponseStatusCode.OK
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
    }
}
