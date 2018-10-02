using System;

namespace Improbable.Gdk.Core.Exceptions
{
    internal class UnknownRequestIdException : Exception
    {
        public UnknownRequestIdException(string message) : base(message)
        {
        }
    }
}
