using System;

namespace EqSolve.Terms
{
    public class IllegalStateException : ApplicationException
    {
        public readonly string Cause;

        public IllegalStateException(string cause)
        {
            Cause = cause;
        }
    }

    public class InterpreterError : ApplicationException
    {
        public readonly string Cause;

        public InterpreterError(string cause)
        {
            Cause = cause;
        }
    }
}