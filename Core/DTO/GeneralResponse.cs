﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO
{
    public class Generalresponse
    {
        public bool IsSuccess { get; set; }
        public dynamic? Data { get; set; }
    }
}
