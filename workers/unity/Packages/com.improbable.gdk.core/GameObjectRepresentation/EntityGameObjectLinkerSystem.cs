using Improbable.Gdk.Core;
using Unity.Entities;

namespace Improbable.Gdk.Core.GameObjectRepresentation
{
    [DisableAutoCreation]
    public class EntityGameObjectLinkerSystem : ComponentSystem
    {
        public EntityGameObjectLinker Linker;

        [Inject] private WorkerSystem worker;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            Linker = new EntityGameObjectLinker(World, worker);
            Enabled = false;
        }

        protected override void OnUpdate()
        {
        }
    }
}
