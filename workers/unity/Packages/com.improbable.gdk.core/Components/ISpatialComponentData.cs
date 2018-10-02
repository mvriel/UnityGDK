using Improbable.Gdk.Core.Utility;

namespace Improbable.Gdk.Core.Components
{
    public interface ISpatialComponentData
    {
        uint ComponentId { get; }
        BlittableBool DirtyBit { get; set; }
    }
}
