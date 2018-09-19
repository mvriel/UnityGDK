using System.Collections.ObjectModel;
using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker;
using Improbable.Worker.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Improbable.Gdk.GameObjectCreation
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SpatialOSReceiveGroup.GameObjectInitializationGroup))]
    public class WorkerEntityGameObjectLinkerSystem : ComponentSystem
    {
        private struct WorkerConnectedData
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<WorkerEntityTag> DenotesWorkerEntity;
            [ReadOnly] public ComponentDataArray<OnConnected> DenotesJustConnected;
            public EntityArray Entities;
        }

        private struct WorkerDisconnectedData
        {
            public readonly int Length;
            [ReadOnly] public ComponentDataArray<WorkerEntityTag> DenotesWorkerEntity;
            [ReadOnly] public SharedComponentDataArray<OnDisconnected> DenotesJustDisconnected;
            public EntityArray Entities;
        }

        [Inject] private WorkerConnectedData connectedData;
        [Inject] private WorkerDisconnectedData disconnectedData;

        [Inject] private EntityGameObjectLinkerSystem linkerSystem;
        [Inject] private WorkerSystem workerSystem;

        private ViewCommandBuffer viewCommandBuffer;
        private GameObject gameObjectToLink;

        public WorkerEntityGameObjectLinkerSystem(GameObject workerGameObject)
        {
            gameObjectToLink = workerGameObject;
        }

        protected override void OnCreateManager()
        {
            viewCommandBuffer = new ViewCommandBuffer(EntityManager, workerSystem.LogDispatcher);
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < connectedData.Length; i++)
            {
                linkerSystem.Linker.LinkGameObjectToEntity(gameObjectToLink, connectedData.Entities[i],
                    new EntityId(0), viewCommandBuffer);
            }

            for (int i = 0; i < disconnectedData.Length; i++)
            {
                linkerSystem.Linker.UnlinkGameObjectFromEntity(gameObjectToLink, disconnectedData.Entities[i],
                    viewCommandBuffer);
            }

            viewCommandBuffer.FlushBuffer();
        }
    }
}
