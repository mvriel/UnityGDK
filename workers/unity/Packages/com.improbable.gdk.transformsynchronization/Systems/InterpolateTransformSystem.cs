using Improbable.Gdk.Core;
using Improbable.Transform;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

#region Diagnostic control

#pragma warning disable 649
#pragma warning disable 169
// ReSharper disable UnassignedReadonlyField
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

#endregion


namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class InterpolateTransformSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<BufferedTransform> TransformBuffer;
            public ComponentDataArray<DefferedUpdateTransform> LastTransformValue;
            public ComponentDataArray<TicksSinceLastTransformUpdate> TicksSinceLastUpdate;
            [ReadOnly] public ComponentDataArray<TransformInternal.ReceivedUpdates> Updates;
            [ReadOnly] public ComponentDataArray<TransformInternal.Component> CurrentTransform;
            [ReadOnly] public ComponentDataArray<NotAuthoritative<TransformInternal.Component>> DenotesNotAuthoritative;
        }

        [Inject] private Data data;
        // todo enable smear
        // [Inject] private TickRateEstimationSystem tickRateSystem;

        // todo this does everything eagerly, but with smearing it will probably make more sense to do things lazily
        // the main difference would be how to detect the buffer is full and needs to be emptied
        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                var transformBuffer = data.TransformBuffer[i].Elements;
                var lastTransformApplied = data.LastTransformValue[i].Transform;

                // todo enable smear
                // Need to take smear into account here when it's turned on
                if (transformBuffer.Count >= TransformSynchronizationConfig.MaxLoadMatchedBufferSize)
                {
                    transformBuffer.Clear();
                }

                if (transformBuffer.Count == 0)
                {
                    var currentTransformComponent = data.CurrentTransform[i];
                    if (currentTransformComponent.PhysicsTick <= lastTransformApplied.PhysicsTick)
                    {
                        continue;
                    }

                    data.LastTransformValue[i] = new DefferedUpdateTransform
                    {
                        Transform = currentTransformComponent
                    };

                    var transformToInterpolateTo = ToBufferedTransform(currentTransformComponent);

                    float tickSmearFactor = 1.0f;
                    // todo enable smear
                    // math.min(lastTransformApplied.TicksPerSecond / tickRateSystem.PhysicsTicksPerRealSecond,
                    //     TransformSynchronizationConfig.MaxTickSmearFactor);

                    uint ticksToFill = (uint)math.max(
                         (TransformSynchronizationConfig.TargetLoadMatchedBufferSize * tickSmearFactor), 1);

                    if (ticksToFill > 1)
                    {
                        var transformToInterpolateFrom = ToBufferedTransformAtTick(lastTransformApplied,
                            transformToInterpolateTo.PhysicsTick - ticksToFill + 1);

                        transformBuffer.Add(transformToInterpolateFrom);

                        for (uint j = 0; j < ticksToFill - 2; ++j)
                        {
                            transformBuffer.Add(InterpolateValues(transformToInterpolateFrom, transformToInterpolateTo,
                                j + 1));
                        }
                    }

                    transformBuffer.Add(transformToInterpolateTo);
                    continue;
                }

                foreach (var update in data.Updates[i].Updates)
                {
                    UpdateLastTransfrom(ref lastTransformApplied, update);
                    data.LastTransformValue[i] = new DefferedUpdateTransform
                    {
                        Transform = lastTransformApplied
                    };

                    // todo: Need to deal with the case where the last tick is back in time. But currently can't happen
                    if (!update.PhysicsTick.HasValue)
                    {
                        continue;
                    }

                    data.TicksSinceLastUpdate[i] = new TicksSinceLastTransformUpdate
                    {
                        NumberOfTicks = 0
                    };

                    float tickSmearFactor = 1.0f;
                    // todo enable smear
                    // math.min(lastTransformApplied.TicksPerSecond / tickRateSystem.PhysicsTicksPerRealSecond,
                    //     TransformSynchronizationConfig.MaxTickSmearFactor);

                    var transformToInterpolateTo = ToBufferedTransform(lastTransformApplied);

                    var transformToInterpolateFrom = transformBuffer[transformBuffer.Count - 1];
                    uint lastTickId = transformToInterpolateFrom.PhysicsTick;

                    uint remoteTickDifference = transformToInterpolateTo.PhysicsTick - lastTickId;
                    if (remoteTickDifference == 0)
                    {
                        continue;
                    }

                    uint ticksToFill =
                        (uint) math.max( ((transformToInterpolateTo.PhysicsTick - lastTickId) * tickSmearFactor), 1);
                    for (uint j = 0; j < ticksToFill - 1; ++j)
                    {
                        transformBuffer.Add(InterpolateValues(transformToInterpolateFrom, transformToInterpolateTo,
                            j + 1));
                    }

                    transformBuffer.Add(transformToInterpolateTo);
                }
            }
        }

        private void UpdateLastTransfrom(ref TransformInternal.Component lastTransform, TransformInternal.Update update)
        {
            if (update.Location.HasValue)
            {
                lastTransform.Location = update.Location.Value;
            }

            if (update.Rotation.HasValue)
            {
                lastTransform.Rotation = update.Rotation.Value;
            }

            if (update.Velocity.HasValue)
            {
                lastTransform.Velocity = update.Velocity.Value;
            }

            if (update.TicksPerSecond.HasValue)
            {
                lastTransform.TicksPerSecond = update.TicksPerSecond.Value;
            }

            if (update.PhysicsTick.HasValue)
            {
                lastTransform.PhysicsTick = update.PhysicsTick.Value;
            }
        }

        private static BufferedTransformElement ToBufferedTransform(TransformInternal.Component transform)
        {
            return new BufferedTransformElement
            {
                Position = transform.Location.ToUnityVector3(),
                Velocity = transform.Velocity.ToUnityVector3(),
                Orientation = transform.Rotation.ToUnityQuaternion(),
                PhysicsTick = transform.PhysicsTick
            };
        }

        private static BufferedTransformElement ToBufferedTransformAtTick(TransformInternal.Component component, uint tick)
        {
            return new BufferedTransformElement
            {
                Position = component.Location.ToUnityVector3(),
                Velocity = component.Velocity.ToUnityVector3(),
                Orientation = component.Rotation.ToUnityQuaternion(),
                PhysicsTick = tick
            };
        }

        private static BufferedTransformElement InterpolateValues(BufferedTransformElement first, BufferedTransformElement second,
            uint ticksAfterFirst)
        {
            float t = (float) ticksAfterFirst / (float) (second.PhysicsTick - first.PhysicsTick);
            return new BufferedTransformElement
            {
                Position = Vector3.Lerp(first.Position, second.Position, t),
                Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t),
                Orientation = Quaternion.Slerp(first.Orientation, second.Orientation, t),
                PhysicsTick = ticksAfterFirst
            };
        }
    }
}
