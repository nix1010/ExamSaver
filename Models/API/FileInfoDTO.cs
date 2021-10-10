using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class FileInfoDTO
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
    }
}
