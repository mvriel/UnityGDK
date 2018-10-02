using System.Collections.Generic;
using Improbable.Gdk.Core.Components;
using Unity.Entities;

namespace Improbable.Gdk.Core.AuthorityHelpers
{
    public struct AuthorityChanges<T> : IComponentData where T : ISpatialComponentData
    {
        public uint Handle;

        public List<Improbable.Worker.Core.Authority> Changes
        {
            get => AuthorityChangesProvider.Get(Handle);
            set => AuthorityChangesProvider.Set(Handle, value);
        }
    }
}
