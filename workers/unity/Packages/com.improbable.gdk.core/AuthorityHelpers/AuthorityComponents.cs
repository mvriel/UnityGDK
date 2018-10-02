using Improbable.Gdk.Core.Components;
using Unity.Entities;

namespace Improbable.Gdk.Core.AuthorityHelpers
{
    public struct Authoritative<T> : IComponentData where T : ISpatialComponentData
    {
    }

    public struct NotAuthoritative<T> : IComponentData where T : ISpatialComponentData
    {
    }

    public struct AuthorityLossImminent<T> : IComponentData where T : ISpatialComponentData
    {
    }
}
