using Improbable.Worker;
using Unity.Entities;

namespace Improbable.Gdk.Core.Components
{
    public struct SpatialEntityId : IComponentData
    {
        public EntityId EntityId;
    }
}
