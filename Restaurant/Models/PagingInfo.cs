using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliEns.Models
{
    public class PagingInfo
    {
        public int TotalRecoreds { get; set; }
        public int RecoredsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalRecoreds / RecoredsPerPage);
        public string urlParam { get; set; }
    }
}
