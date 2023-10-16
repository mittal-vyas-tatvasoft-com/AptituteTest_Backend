﻿using AptitudeTest.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AptitudeTest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        #region Properties
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        #region Methods

        #region GetAllUsers
        /// <summary>
        /// This method fetches all the users with pagination data according to search string
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="currentPageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("[action]/{currentPageIndex:int}/{pageSize:int}")]
        public async Task<JsonResult> GetAllUsers(string? searchQuery, int? currentPageIndex = 0, int? pageSize = 10)
        {
            return await _userService.GetAllUsers(searchQuery, currentPageIndex, pageSize);
        }
        #endregion


        #region GetUserByIdUsingDapper
        /// <summary>
        /// This method fetches single user data using user's Id
        /// </summary>
        /// <param name="id">user will be fetched according to this 'id'</param>
        /// <returns> user </returns> 
        [HttpGet("[action]/{id:int}")]
        public async Task<JsonResult> GetUserByIdUsingDapper(int id)
        {
            return await _userService.GetUserByIdUsingDapper(id);
        }
        #endregion
        #endregion
    }
}
