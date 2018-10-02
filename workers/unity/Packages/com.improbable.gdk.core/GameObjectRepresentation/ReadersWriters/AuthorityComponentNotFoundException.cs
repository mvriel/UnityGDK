using System;

namespace Improbable.Gdk.Core.GameObjectRepresentation.ReadersWriters
{
    public class AuthorityComponentNotFoundException : Exception
    {
        public AuthorityComponentNotFoundException(string message) : base(message)
        {
        }
    }
}
