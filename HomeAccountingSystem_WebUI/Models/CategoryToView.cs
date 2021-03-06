﻿using System.Collections.Generic;
using HomeAccountingSystem_DAL.Model;

namespace HomeAccountingSystem_WebUI.Models
{
    public class CategoriesViewModel
    {
        public List<Category> Categories { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int TypeOfFlowId { get; set; }
    }
}