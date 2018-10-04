using System.Collections.Generic;
using Improbable.Gdk.Core;

namespace Improbable.Gdk.PlayerLifecycle
{
    public delegate EntityTemplate GetPlayerEntityTemplateDelegate(
        string clientWorkerId,
        List<string> clientAttributeSet,
        global::Improbable.PlayerLifecycle.CreatePlayerRequestType payload
    );

    public delegate global::Improbable.PlayerLifecycle.CreatePlayerRequestType GetPlayerCreationDataDelegate();

    public static class PlayerLifecycleConfig
    {
        public const float PlayerHeartbeatIntervalSeconds = 5f;
        public const int MaxNumFailedPlayerHeartbeats = 2;

        public static GetPlayerEntityTemplateDelegate CreatePlayerEntityTemplate;
        public static GetPlayerCreationDataDelegate GetPlayerCreationData;
    }
}
