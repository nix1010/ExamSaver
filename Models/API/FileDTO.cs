using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public abstract class FileDTO
    {
        public string Name { get; set; }
        public long Size { get; set; }
    }
}
