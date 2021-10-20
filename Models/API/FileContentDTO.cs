using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class FileContentDTO : FileDTO
    {
        public string Content { get; set; }
    }
}
