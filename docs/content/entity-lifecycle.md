**Warning:** The [alpha](https://docs.improbable.io/reference/latest/shared/release-policy#maturity-stages) release is for evaluation purposes only, with limited documentation - see the guidance on [Recommended use](../../README.md#recommended-use).

---
## The entity lifecycle

The SpatialOS runtime manages the lifecycle of your entities for you. The SpatialOS GDK for Unity interacts with the SpatialOS runtime through [Operations](https://docs.improbable.io/reference/latest/shared/design/operations#operations-how-workers-communicate-with-spatialos) and integrates this lifecycle natively into Unity.

Central to this lifecycle is the concept of your worker's [view](https://docs.improbable.io/reference/latest/shared/concepts/interest-authority#server-worker-view), or the part of the game world that your worker has access to. From the point of view of your worker, the natural lifecycle of an entity is dictated by the entity entering and leaving your worker's view.

### What happens when an entity enters your view

When an entity moves into your worker's [view](https://docs.improbable.io/reference/latest/shared/concepts/interest-authority#server-worker-view), the SpatialOS runtime sends a set of operations to your worker describing the current state of that entity. For a single entity, your worker will receive:

 - An `AddEntityOp`
 - An `AddComponentOp` for each SpatialOS component on that entity
 - Optionally, an `AuthorityChangeOp` per component if your worker is delegated [authority](https://docs.improbable.io/reference/latest/shared/concepts/interest-authority#authority) over that component

The SpatialOS GDK for Unity turns these ops into a single ECS Entity in a process described in the [Entity Checkout Process](TODO). You can also optionally create a GameObject to represent this entity as described [in this doc](TODO).

### What happens when an entity leaves your view

When an entity moves out of your worker's [checkout region](https://docs.improbable.io/reference/latest/shared/concepts/workers-load-balancing), the SpatialOS runtime sends a set of operations to your worker to represent that change. For a single entity, your worker will receive:

- A `RemoveComponentOp` for each SpatialOS component on that entity
- A `RemoveEntityOp` 

The SpatialOS GDK for Unity uses these ops to remove the ECS Entity and clean up any data associated with it. If you chose to create a GameObject to represent this entity, you will receive a callback before this GameObject is removed.

## Interacting with the entity lifecycle

The SpatialOS runtime also provides an API for interacting with the Entity Lifecycle. This allows you to create and delete entities from your worker.

### Creating entities

If you want to create a SpatialOS entity from a worker, you need to use the `Create Entity` world command. See [World Commands - Monobehaviours](TODO) or [World Commands - ECS](TODO) for more information on how to do that.

> Note that creating a local entity in Unity does not mean it will be replicated to SpatialOS. Only entities created through the `Create Entity` will be SpatialOS entities.

Once the `Create Entity` world command succeeds, you will receive the Entity from SpatialOS through the [standard checkout process](TODO). 

### Deleting entities

If you want to delete a SpatialOS entity, you need to use the `Delete Entity` world command. See [World Commands - Monobehaviours](TODO) or [World Commands - ECS](TODO) for more information on how to do that.

> Note that deleting a local entity in Unity that corresponds to a SpatialOS entity will not delete it from the SpatialOS runtime. Only entities deleted through the `Delete Entity` world command will be deleted on the SpatialOS runtime. 

Once the `Delete Entity` world command succeeds, the entity will be removed from your worker by the SpatialOS GDK for Unity.

> Warning: Manually deleting entities locally will cause runtime errors. 

-----

**Give us feedback:** We want your feedback on the SpatialOS GDK for Unity and its documentation  - see [How to give us feedback](../../../README.md#give-us-feedback).