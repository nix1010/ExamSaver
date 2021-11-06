using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class SimilarityRequestDTO
    {
        public string FileExtension { get; set; }
        public string Comment { get; set; }
    }
}
