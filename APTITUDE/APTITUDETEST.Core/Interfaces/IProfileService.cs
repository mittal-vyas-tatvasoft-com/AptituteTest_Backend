﻿using AptitudeTest.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AptitudeTest.Core.Interfaces
{
    public interface IProfileService
    {
        public Task<JsonResult> GetProfiles();
        public Task<JsonResult> GetActiveProfiles();

        public Task<JsonResult> Create(ProfileVM profile);
        public Task<JsonResult> Update(ProfileVM profile);

        public Task<JsonResult> CheckUncheckAll(bool check);

        public Task<JsonResult> Delete(int id);

        public Task<JsonResult> GetProfileById(int? id);

        public Task<JsonResult> UpdateStatus(StatusVM status);
    }
}
