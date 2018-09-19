using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Improbable.Gdk.TransformSynchronization
{
    public class BufferedTransform : Component
    {
        public List<BufferedTransformElement> Elements;
    }

    public struct BufferedTransformElement : IComponentData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Orientation;
        public uint PhysicsTick;
    }
}
