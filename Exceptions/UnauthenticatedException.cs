using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Exceptions
{
    public class UnauthenticatedException : Exception
    {
        public UnauthenticatedException(string message) : base(message) { }
        public UnauthenticatedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
