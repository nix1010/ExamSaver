using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models
{
    public class AppSettings
    {
        public string JWTSecretKey { get; set; }
        public string ExamsDirectoryPath { get; set; }
    }
}
