using System;

namespace Improbable.Gdk.Core.Exceptions
{
    internal class UnknownComponentIdException : Exception
    {
        public UnknownComponentIdException(string message) : base(message)
        {
        }
    }
}

