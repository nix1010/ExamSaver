using ExamSaver.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Services.Interfaces
{
    public interface ISimilarityService
    {
        public IList<SimilarityResultDTO> GetSimilarityResults(int userId, int examId);

        public void DeleteSimilarityResult(int userId, int examId, int similarityResultId);

        public SimilarityRunResultDTO PerformSimilarityCheck(int userId, int examId, SimilarityRequestDTO similarityRequestDTO);
    }
}
