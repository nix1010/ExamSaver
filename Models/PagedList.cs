using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models
{
    public class PagedList<T> : List<T>
    {
        public Page Page { get; set; }

        public PagedList(Page page) : this(null, page) { }

        public PagedList(IEnumerable<T> enumerable, Page page) : base(enumerable)
        {
            Page = page;
        }
    }
}
