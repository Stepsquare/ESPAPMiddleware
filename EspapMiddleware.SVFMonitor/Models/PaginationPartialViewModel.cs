using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EspapMiddleware.SVFMonitor.Models
{
    public class PaginationPartialViewModel
    {
        public PaginationPartialViewModel(int pageIndex, int pageSize, int totalCount, string ajaxRequestMethod)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (decimal)PageSize);
            AjaxRequestMethod = ajaxRequestMethod;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string AjaxRequestMethod { get; set; }
    }
}