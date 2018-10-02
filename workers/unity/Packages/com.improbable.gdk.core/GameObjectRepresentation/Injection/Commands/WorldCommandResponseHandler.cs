using System;
using System.Collections.Generic;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.GameObjectRepresentation.Injection;
using Improbable.Gdk.Core.GameObjectRepresentation.ReadersWriters;
using Improbable.Gdk.Core.Logging;
using Unity.Entities;
using Entity = Unity.Entities.Entity;

#region Diagnostic control

// ReSharper disable UnusedMember.Local

#endregion


namespace Improbable.Gdk.Core.GameObjectRepresentation.Injection.Commands
{
    public static partial class WorldCommands
    {
        public static partial class Requirable
        {
            [InjectableId(InjectableType.WorldCommandResponseHandler, InjectableId.NullComponentId)]
            [InjectionCondition(InjectionCondition.RequireNothing)]
            public class WorldCommandResponseHandler : RequirableBase
            {
                private readonly ILogDispatcher logDispatcher;

                private readonly List<Action<Core.Commands.WorldCommands.ReserveEntityIds.ReceivedResponse>>
                    reserveEntityIdsDelegates;

                private readonly List<Action<Core.Commands.WorldCommands.CreateEntity.ReceivedResponse>>
                    createEntityDelegates;

                private readonly List<Action<Core.Commands.WorldCommands.DeleteEntity.ReceivedResponse>>
                    deleteEntityDelegates;

                private readonly List<Action<Core.Commands.WorldCommands.EntityQuery.ReceivedResponse>>
                    entityQueryDelegates;

                private WorldCommandResponseHandler(ILogDispatcher logDispatcher) : base(logDispatcher)
                {
                    this.logDispatcher = logDispatcher;
                    reserveEntityIdsDelegates =
                        new List<Action<Core.Commands.WorldCommands.ReserveEntityIds.ReceivedResponse>>();
                    createEntityDelegates =
                        new List<Action<Core.Commands.WorldCommands.CreateEntity.ReceivedResponse>>();
                    deleteEntityDelegates =
                        new List<Action<Core.Commands.WorldCommands.DeleteEntity.ReceivedResponse>>();
                    entityQueryDelegates = new List<Action<Core.Commands.WorldCommands.EntityQuery.ReceivedResponse>>();
                }

                public event Action<Core.Commands.WorldCommands.ReserveEntityIds.ReceivedResponse>
                    OnReserveEntityIdsResponse
                    {
                        add
                        {
                            if (!VerifyNotDisposed())
                            {
                                return;
                            }

                            reserveEntityIdsDelegates.Add(value);
                        }
                        remove
                        {
                            if (!VerifyNotDisposed())
                            {
                                return;
                            }

                            reserveEntityIdsDelegates.Remove(value);
                        }
                    }

                public event Action<Core.Commands.WorldCommands.CreateEntity.ReceivedResponse> OnCreateEntityResponse
                {
                    add
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        createEntityDelegates.Add(value);
                    }
                    remove
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        createEntityDelegates.Remove(value);
                    }
                }

                public event Action<Core.Commands.WorldCommands.DeleteEntity.ReceivedResponse> OnDeleteEntityResponse
                {
                    add
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        deleteEntityDelegates.Add(value);
                    }
                    remove
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        deleteEntityDelegates.Remove(value);
                    }
                }

                public event Action<Core.Commands.WorldCommands.EntityQuery.ReceivedResponse> OnEntityQueryResponse
                {
                    add
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        entityQueryDelegates.Add(value);
                    }
                    remove
                    {
                        if (!VerifyNotDisposed())
                        {
                            return;
                        }

                        entityQueryDelegates.Remove(value);
                    }
                }

                internal void OnReserveEntityIdsResponseInternal(
                    Core.Commands.WorldCommands.ReserveEntityIds.ReceivedResponse receivedResponse)
                {
                    GameObjectDelegates.DispatchWithErrorHandling(receivedResponse, reserveEntityIdsDelegates,
                        logDispatcher);
                }

                internal void OnCreateEntityResponseInternal(
                    Core.Commands.WorldCommands.CreateEntity.ReceivedResponse receivedResponse)
                {
                    GameObjectDelegates.DispatchWithErrorHandling(receivedResponse, createEntityDelegates,
                        logDispatcher);
                }

                internal void OnDeleteEntityResponseInternal(
                    Core.Commands.WorldCommands.DeleteEntity.ReceivedResponse receivedResponse)
                {
                    GameObjectDelegates.DispatchWithErrorHandling(receivedResponse, deleteEntityDelegates,
                        logDispatcher);
                }

                internal void OnEntityQueryResponseInternal(
                    Core.Commands.WorldCommands.EntityQuery.ReceivedResponse receivedResponse)
                {
                    GameObjectDelegates.DispatchWithErrorHandling(receivedResponse, entityQueryDelegates,
                        logDispatcher);
                }

                [InjectableId(InjectableType.WorldCommandResponseHandler, InjectableId.NullComponentId)]
                private class WorldCommandResponseHandlerCreator : IInjectableCreator
                {
                    public IInjectable CreateInjectable(Entity entity, EntityManager entityManager,
                        ILogDispatcher logDispatcher)
                    {
                        return new WorldCommandResponseHandler(logDispatcher);
                    }
                }
            }
        }
    }
}
