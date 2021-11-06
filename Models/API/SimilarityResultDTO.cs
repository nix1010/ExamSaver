using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class SimilarityResultDTO
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
        public DateTime Submitted { get; set; }

        public static SimilarityResultDTO FromEntity(SimilarityResult similarityResult)
        {
            return new SimilarityResultDTO()
            {
                Id = similarityResult.Id,
                Url = similarityResult.Url,
                Comment = similarityResult.Comment,
                Submitted = similarityResult.Submitted
            };
        }
    }
}
