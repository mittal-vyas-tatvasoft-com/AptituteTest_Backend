﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptitudeTest.Core.ViewModels
{
    public class TempUserResultsVM
    {
        public int Id { get; set; }
        public int UserTestId { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }
        public bool IsActive { get; set; }
    }
}
