using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class FileInfoDTO : FileDTO
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
    }
}
