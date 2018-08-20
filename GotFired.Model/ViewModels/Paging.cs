using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.ViewModels
{
    public class Paging
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        //public int TotalPages
        //{
        //    get
        //    {
        //        return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
        //    }
        //}
        //public int ShowPageNumber { get; set; }
        //public int ShowPageNumberStart
        //{
        //    get
        //    {
        //        if (CurrentPage - ShowPageNumber < 0)
        //            return 0;
        //        return CurrentPage - ShowPageNumber;
        //    }
        //}

        //public int ShowPageNumberEnd
        //{
        //    get
        //    {
        //        if (CurrentPage + ShowPageNumber > TotalPages)
        //            return TotalPages;
        //        return CurrentPage + ShowPageNumber;
        //    }
        //}

        //public int PreviousPage
        //{
        //    get
        //    {
        //        if (CurrentPage - 1 < 0)
        //            return 0;
        //        return CurrentPage - 1;
        //    }
        //}

        //public int NextPage
        //{
        //    get
        //    {
        //        if (CurrentPage + 1 >= TotalPages - 1)
        //            return TotalPages - 1;
        //        return CurrentPage + 1;
        //    }
        //}

    }
}
