using Latios;
using Latios.Psyshock;
using Scenes.UniqueMeshTests.UniqueMeshTests;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Physics = Latios.Psyshock.Physics;

namespace CollisionWorldTest
{
    public partial class CollisionWorldSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<BuildCollisionWorldSystem>();
            GetOrCreateAndAddUnmanagedSystem<FindCollisionPairsSystem>();
        }
    }
    
    
    public struct BodyOne : IComponentData { }
    public struct BodyTwo : IComponentData { }
    
    public partial struct CollisionWorldHolder : ICollectionComponent
    {
        public CollisionWorld World;

        public JobHandle TryDispose(JobHandle inputDeps) => World.IsCreated ? World.Dispose(inputDeps) : inputDeps;
    }
    
    [BurstCompile]
    public partial struct BuildCollisionWorldSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged latiosWorld;

        EntityQuery query;
        BuildCollisionWorldTypeHandles handles;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            latiosWorld = state.GetLatiosWorldUnmanaged();
            handles = new BuildCollisionWorldTypeHandles(ref state);
            query = state.Fluent()
                .WithAnyEnabled<BodyOne, BodyTwo>(true)
                .PatchQueryForBuildingCollisionWorld()
                .Build();
        }

        [BurstCompile]
        public void OnNewScene(ref SystemState state)
        {
            latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld<CollisionWorldHolder>(default);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            handles.Update(ref state);
            state.Dependency = Physics.BuildCollisionWorld(query, in handles)
                // .WithSettings(CollisionSettings.Default)
                .ScheduleParallel(out var collisionWorld, state.WorldUpdateAllocator, state.Dependency);
            latiosWorld.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new CollisionWorldHolder { World = collisionWorld });
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            latiosWorld.sceneBlackboardEntity.RemoveCollectionComponentAndDispose<CollisionWorldHolder>();
        }
    }

    [BurstCompile]
    public partial struct FindCollisionPairsSystem : ISystem, ISystemShouldUpdate
    {
        LatiosWorldUnmanaged latiosWorld;
        EntityQueryMask bodiesOneQueryMask;
        EntityQueryMask bodiesTwoQueryMask;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            latiosWorld = state.GetLatiosWorldUnmanaged();
            bodiesOneQueryMask = state.Fluent().With<BodyOne>().Build().GetEntityQueryMask();
            bodiesTwoQueryMask = state.Fluent().With<BodyTwo>().Build().GetEntityQueryMask();
        }

        [BurstCompile]
        public bool ShouldUpdateSystem(ref SystemState state) => latiosWorld.sceneBlackboardEntity.HasCollectionComponent<CollisionWorldHolder>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<CollisionWorldHolder>(true).World;

            state.Dependency = Physics.FindPairs(in collisionWorld, in bodiesOneQueryMask, in bodiesTwoQueryMask, new OneVsTwoBodiesProcessor
            {
            }).ScheduleSingle(state.Dependency);

            // state.Dependency = PhysicsDebug.DrawFindPairs(collisionWorld.collisionLayer).ScheduleParallel(state.Dependency);

            state.Dependency = Physics.FindPairs(in collisionWorld, in bodiesOneQueryMask, new OneVsOneBodiesProcessor
            {
            }).ScheduleSingle(state.Dependency);
        }

        struct OneVsTwoBodiesProcessor : IFindPairsProcessor
        {
            public void Execute(in FindPairsResult result)
            {
                PhysicsDebug.DrawAabb(result.aabbA, Color.red);
                PhysicsDebug.DrawAabb(result.aabbB, Color.red);
            }
        }

        [BurstCompile]
        struct OneVsOneBodiesProcessor : IFindPairsProcessor
        {
            [BurstCompile]
            public void Execute(in FindPairsResult result)
            {
                PhysicsDebug.DrawAabb(result.aabbA, Color.green);
                PhysicsDebug.DrawAabb(result.aabbB, Color.green);
            }
        }
    }
}