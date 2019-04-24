﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewApi.Models.Tameplate
{
    public class Header
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Fcn { get; set; }
        public string Parameter { get; set; }
        public string ColumnName { get; set; }
        public string Data { get; set; }
    }
}
