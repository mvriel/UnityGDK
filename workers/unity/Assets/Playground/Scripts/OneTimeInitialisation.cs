using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Unity.Entities;
using UnityEngine;
using Improbable.PlayerLifecycle;

namespace Playground
{
    public static class OneTimeInitialisation
    {
        private static bool initialized;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            WorldsInitializationHelper.SetupInjectionHooks();
            PlayerLoopManager.RegisterDomainUnload(WorldsInitializationHelper.DomainUnloadShutdown, 1000);

            // Setup template to use for player on connecting client
            PlayerLifecycleConfig.CreatePlayerEntityTemplate = PlayerTemplate.CreatePlayerEntityTemplate;
            PlayerLifecycleConfig.CreatePlayerEntityTemplate = PlayerTemplate.CreatePlayerEntityTemplate;
        }

        private static CreatePlayerRequestType GetPlayerCreationData()
        {
            return new CreatePlayerRequestType(new Vector3f(0, 0, 0), new Dictionary<string, string>());
        }
    }
}
