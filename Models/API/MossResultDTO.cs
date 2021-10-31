using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class MossResultDTO
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime Submitted { get; set; }

        public static MossResultDTO FromEntity(MossResult mossResult)
        {
            return new MossResultDTO()
            {
                Id = mossResult.Id,
                Url = mossResult.Url,
                Submitted = mossResult.Submitted
            };
        }
    }
}
