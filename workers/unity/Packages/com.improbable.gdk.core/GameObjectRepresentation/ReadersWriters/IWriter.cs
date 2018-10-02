using Improbable.Gdk.Core.Components;

namespace Improbable.Gdk.Core.GameObjectRepresentation.ReadersWriters
{
    public interface IWriter<TSpatialComponentData, TComponentUpdate>
        : IReader<TSpatialComponentData, TComponentUpdate>
        where TSpatialComponentData : ISpatialComponentData
        where TComponentUpdate : ISpatialComponentUpdate
    {
        void Send(TComponentUpdate update);
    }
}
