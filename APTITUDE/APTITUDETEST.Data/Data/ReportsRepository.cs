﻿using AptitudeTest.Core.Interfaces;
using AptitudeTest.Core.ViewModels;
using AptitudeTest.Data.Common;
using APTITUDETEST.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static AptitudeTest.Data.Common.Enums;

namespace AptitudeTest.Data.Data
{
    public class ReportsRepository : IReportsRepository
    {
        #region Properties
        private readonly string? connectionString;
        private readonly string? root;
        private readonly string? parent;
        private readonly string? screenshot;
        private readonly string? faceCam;
        private AppDbContext _context;
        #endregion

        #region Constructor
        public ReportsRepository(AppDbContext context, IConfiguration config)
        {
            IConfiguration _config;
            _config = config;
            connectionString = _config["ConnectionStrings:AptitudeTest"];
            root = _config["FileSavePath:Root"];
            parent = _config["FileSavePath:ParentFolder"];
            screenshot = _config["FileSavePath:ScreenShot"];
            faceCam = _config["FileSavePath:UserFaceCam"];
            _context = context;
        }
        #endregion

        #region Methods
        public async Task<JsonResult> GetScreenShots(int userId, int testId, int imageType)
        {
            try
            {
                List<string> imagePaths = new List<string>();
                string path = Path.Combine(root, parent, testId.ToString(), userId.ToString());
                if (Directory.Exists(path))
                {
                    if (imageType == (int)ImageType.ScreenShot)
                    {
                        string finalPath = Path.Combine(path, screenshot);
                        if (Directory.Exists(finalPath))
                        {
                            imagePaths = Directory.GetFiles(finalPath).Select(d => Path.GetRelativePath(finalPath, d)).ToList();
                            return new JsonResult(new ApiResponse<List<string>>() { Data = imagePaths, Message = ResponseMessages.Success, Result = true, StatusCode = ResponseStatusCode.Success });

                        }
                        return new JsonResult(new ApiResponse<string>
                        {
                            Message = ResponseMessages.BadRequest,
                            Result = false,
                            StatusCode = ResponseStatusCode.BadRequest
                        });
                    }

                    else if (imageType == (int)ImageType.FaceCam)
                    {
                        string finalPath = Path.Combine(path, faceCam);
                        if (Directory.Exists(finalPath))
                        {
                            imagePaths = Directory.GetFiles(finalPath).Select(d => Path.GetRelativePath(finalPath, d)).ToList();
                            return new JsonResult(new ApiResponse<List<string>>() { Data = imagePaths, Message = ResponseMessages.Success, Result = true, StatusCode = ResponseStatusCode.Success });
                        }
                        return new JsonResult(new ApiResponse<string>
                        {
                            Message = ResponseMessages.BadRequest,
                            Result = false,
                            StatusCode = ResponseStatusCode.BadRequest
                        });
                    }

                    else
                    {
                        return new JsonResult(new ApiResponse<string>
                        {
                            Message = ResponseMessages.BadRequest,
                            Result = false,
                            StatusCode = ResponseStatusCode.BadRequest
                        });
                    }

                }
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.NotFound, ModuleNames.Reports),
                    Result = false,
                    StatusCode = ResponseStatusCode.NotFound
                });
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

        public async Task<JsonResult> GetTests()
        {
            try
            {
                List<int> testDirectories = new List<int>();
                List<ScreenShotFolderVM> screenShotFolderVM = new List<ScreenShotFolderVM>();
                string parentDirectory = Path.Combine(root, parent);
                if (Directory.Exists(parentDirectory))
                {
                    testDirectories = Directory.GetDirectories(parentDirectory).Select(d => Int32.TryParse(Path.GetRelativePath(parentDirectory, d), out int id) ? id : 0).ToList();
                    screenShotFolderVM = _context.Tests.Where(t => testDirectories.Contains(t.Id)).Select(t => new ScreenShotFolderVM() { Id = t.Id, Name = t.Name }).ToList();
                    return new JsonResult(new ApiResponse<List<ScreenShotFolderVM>>() { Data = screenShotFolderVM, Message = ResponseMessages.Success, Result = true, StatusCode = ResponseStatusCode.Success });
                }
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.NotFound, ModuleNames.Directory),
                    Result = false,
                    StatusCode = ResponseStatusCode.NotFound
                });
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

        public async Task<JsonResult> GetUsers(int testId)
        {
            try
            {
                List<int> userFolders = new List<int>();
                List<ScreenShotFolderVM> screenShotFolderVM = new List<ScreenShotFolderVM>();
                string parentDirectory = Path.Combine(root, parent, testId.ToString());
                if (Directory.Exists(parentDirectory))
                {
                    userFolders = Directory.GetDirectories(parentDirectory).Select(d => Int32.TryParse(Path.GetRelativePath(parentDirectory, d), out int id) ? id : 0).ToList();
                    screenShotFolderVM = _context.Users.Where(t => userFolders.Contains(t.Id)).Select(t => new ScreenShotFolderVM() { Id = t.Id, Name = t.FirstName + " " + t.LastName }).ToList();
                    return new JsonResult(new ApiResponse<List<ScreenShotFolderVM>>() { Data = screenShotFolderVM, Message = ResponseMessages.Success, Result = true, StatusCode = ResponseStatusCode.Success });
                }
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.NotFound, ModuleNames.Directory),
                    Result = false,
                    StatusCode = ResponseStatusCode.NotFound
                });
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

        public async Task<JsonResult> GetUserDirectories(int userId, int testId)
        {
            try
            {
                List<string> userDirectories = new List<string>();
                List<ScreenShotFolderVM> screenShotFolderVM = new List<ScreenShotFolderVM>();
                string parentDirectory = Path.Combine(root, parent, testId.ToString(), userId.ToString());
                if (Directory.Exists(parentDirectory))
                {
                    userDirectories = Directory.GetDirectories(parentDirectory).Select(d => Path.GetRelativePath(parentDirectory, d)).ToList();
                    return new JsonResult(new ApiResponse<List<string>>() { Data = userDirectories, Message = ResponseMessages.Success, Result = true, StatusCode = ResponseStatusCode.Success });
                }
                return new JsonResult(new ApiResponse<string>
                {
                    Message = string.Format(ResponseMessages.NotFound, ModuleNames.Directory),
                    Result = false,
                    StatusCode = ResponseStatusCode.NotFound
                });
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
    }
}
