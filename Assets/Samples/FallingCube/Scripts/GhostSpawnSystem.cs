using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

public partial class CubeGhostSpawnSystem : DefaultGhostSpawnSystem<CubeSnapshotData>
{
    protected override EntityArchetype GetGhostArchetype()
    {
        return EntityManager.CreateArchetype(
            ComponentType.ReadWrite<CubeSnapshotData>(),
            ComponentType.ReadWrite<LocalToWorld>(),
            ComponentType.ReadWrite<PerInstanceCullingTag>(),
            ComponentType.ReadWrite<RenderMesh>(),
            ComponentType.ReadWrite<Rotation>(),
            ComponentType.ReadWrite<Translation>(),

            ComponentType.ReadWrite<ReplicatedEntityComponent>()
        );
    }
    protected override EntityArchetype GetPredictedGhostArchetype()
    {
        return EntityManager.CreateArchetype(
            ComponentType.ReadWrite<CubeSnapshotData>(),
            ComponentType.ReadWrite<LocalToWorld>(),
            ComponentType.ReadWrite<PerInstanceCullingTag>(),
            ComponentType.ReadWrite<PhysicsCollider>(),
            ComponentType.ReadWrite<PhysicsDamping>(),
            ComponentType.ReadWrite<PhysicsMass>(),
            ComponentType.ReadWrite<PhysicsVelocity>(),
            ComponentType.ReadWrite<RenderMesh>(),
            ComponentType.ReadWrite<Rotation>(),
            ComponentType.ReadWrite<Translation>(),

            ComponentType.ReadWrite<ReplicatedEntityComponent>(),
            ComponentType.ReadWrite<PredictedEntityComponent>()
        );
    }
}
