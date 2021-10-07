using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models
{
    public static class RoleType
    {
        public const string PROFESSOR = "PROFESSOR";
        public const string STUDENT = "STUDENT";

        public static readonly string[] ALL_ROLES =
        {
            PROFESSOR,
            STUDENT
        };
    }
}
