using Improbable.Gdk.Core.GameObjectRepresentation.ReadersWriters;
using Improbable.Gdk.Core.Logging;
using Improbable.Worker;
using Unity.Entities;

#region Diagnostic control

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

#endregion

namespace Improbable.Gdk.Core.GameObjectRepresentation.Injection.Commands
{
    public static partial class WorldCommands
    {
        public static partial class Requirable
        {
            [InjectableId(InjectableType.WorldCommandRequestSender, InjectableId.NullComponentId)]
            [InjectionCondition(InjectionCondition.RequireNothing)]
            public class WorldCommandRequestSender : RequirableBase
            {
                private readonly Entity entity;
                private readonly EntityManager entityManager;

                private WorldCommandRequestSender(Entity entity, EntityManager entityManager,
                    ILogDispatcher logDispatcher) : base(logDispatcher)
                {
                    this.entity = entity;
                    this.entityManager = entityManager;
                }

                public long ReserveEntityIds(uint numberOfEntityIds, uint? timeoutMillis = null, object context = null)
                {
                    if (!VerifyNotDisposed())
                    {
                        return -1;
                    }

                    var request =
                        Improbable.Gdk.Core.Commands.WorldCommands.ReserveEntityIds.CreateRequest(numberOfEntityIds, timeoutMillis, context);

                    entityManager.GetComponentData<Improbable.Gdk.Core.Commands.WorldCommands.ReserveEntityIds.CommandSender>(entity)
                        .RequestsToSend.Add(request);

                    return request.RequestId;
                }

                public long CreateEntity(Improbable.Worker.Core.Entity entityTemplate, EntityId? entityId = null,
                    uint? timeoutMillis = null, object context = null)
                {
                    if (!VerifyNotDisposed())
                    {
                        return -1;
                    }

                    var request =
                        Improbable.Gdk.Core.Commands.WorldCommands.CreateEntity.CreateRequest(entityTemplate, entityId, timeoutMillis, context);

                    entityManager.GetComponentData<Improbable.Gdk.Core.Commands.WorldCommands.CreateEntity.CommandSender>(entity)
                        .RequestsToSend.Add(request);

                    return request.RequestId;
                }

                public long DeleteEntity(EntityId entityId, uint? timeoutMillis = null, object context = null)
                {
                    if (!VerifyNotDisposed())
                    {
                        return -1;
                    }

                    var request = Improbable.Gdk.Core.Commands.WorldCommands.DeleteEntity.CreateRequest(entityId, timeoutMillis, context);

                    entityManager.GetComponentData<Improbable.Gdk.Core.Commands.WorldCommands.DeleteEntity.CommandSender>(entity)
                        .RequestsToSend.Add(request);

                    return request.RequestId;
                }

                public long EntityQuery(Improbable.Worker.Query.EntityQuery entityQuery, uint? timeoutMillis = null,
                    object context = null)
                {
                    if (!VerifyNotDisposed())
                    {
                        return -1;
                    }

                    var request = Improbable.Gdk.Core.Commands.WorldCommands.EntityQuery.CreateRequest(entityQuery, timeoutMillis, context);
                    entityManager.GetComponentData<Improbable.Gdk.Core.Commands.WorldCommands.EntityQuery.CommandSender>(entity)
                        .RequestsToSend.Add(request);

                    return request.RequestId;
                }

                [InjectableId(InjectableType.WorldCommandRequestSender, InjectableId.NullComponentId)]
                private class WorldCommandRequestSenderCreator : IInjectableCreator
                {
                    public IInjectable CreateInjectable(Entity entity, EntityManager entityManager,
                        ILogDispatcher logDispatcher)
                    {
                        return new WorldCommandRequestSender(entity, entityManager, logDispatcher);
                    }
                }
            }
        }
    }
}
