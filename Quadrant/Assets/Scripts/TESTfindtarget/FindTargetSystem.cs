using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(QuadrantSystem))]
public class FindTargetJobSystem : JobComponentSystem
{
    private struct EntityWithPosition
    {
        public Entity Entity;
        public float3 Position;
    }
    
    [RequireComponentTag(typeof(Target))]
    [BurstCompile]
    private struct FillArrayEntityWithPositionJob : IJobForEachWithEntity<Translation>
    {
        public NativeArray<EntityWithPosition> TargetArray;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            TargetArray[index] = new EntityWithPosition
            {
                Entity = entity,
                Position = translation.Value
            };
        }
    }

    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<EntityWithPosition> TargetArray;
        public NativeArray<Entity> ClosestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            Entity closestTarget = Entity.Null;
            
            float3 unitPosition = translation.Value;
            float closestTargetDistance = float.MaxValue;

            for (int i = 0; i < TargetArray.Length; i++)
            {
                EntityWithPosition targetEntityWithPosition = TargetArray[i];
                
                if (closestTarget == Entity.Null)
                {
                    closestTarget = targetEntityWithPosition.Entity;
                    closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.Position);
                }
                else
                {
                    if (math.distancesq(unitPosition, targetEntityWithPosition.Position) < closestTargetDistance)
                    {
                        closestTarget = targetEntityWithPosition.Entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.Position);
                    }
                }
            }

            ClosestTargetEntityArray[index] = closestTarget;
        }
    }

    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    private struct AddComponentJob : IJobForEachWithEntity<Translation>
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ClosestTargetArray;
        
        public void Execute(Entity entity, int index, ref Translation translation)
        {
            if (ClosestTargetArray[index] != Entity.Null)
            {
                EntityCommandBuffer.AddComponent(index, entity, new HasTarget
                {
                    TargetEntity = ClosestTargetArray[index]
                });
            }
        }
    }

    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> QuadrantMultiHashMap;
        [DeallocateOnJobCompletion] public NativeArray<Entity> ClosestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            Entity closestTarget = Entity.Null;
            
            float3 unitPosition = translation.Value;
            float closestTargetDistance = float.MaxValue;

            int hasMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);

            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

            if (QuadrantMultiHashMap.TryGetFirstValue(hasMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (quadrantEntity.TypeEnum != quadrantData.QuadrantEntity.TypeEnum)
                    {
                        if (closestTarget == Entity.Null)
                        {
                            closestTarget = quadrantData.Entity;
                            closestTargetDistance = math.distancesq(unitPosition, quadrantData.Position);
                        }
                        else
                        {
                            if (math.distancesq(unitPosition, quadrantData.Position) < closestTargetDistance)
                            {
                                closestTarget = quadrantData.Entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.Position);
                            }
                        }
                    }
                } while (QuadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }

            ClosestTargetEntityArray[index] = closestTarget;
        }
    }
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.ReadOnly<Translation>());
        
            EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
            
            NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            FindTargetQuadrantSystemJob findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob
            {
                QuadrantMultiHashMap = QuadrantSystem.QuadrantMultiHashMap,
                ClosestTargetEntityArray = closestTargetEntityArray
            };
            JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(this, inputDeps);

            AddComponentJob addComponentJob = new AddComponentJob
            {
                ClosestTargetArray = closestTargetEntityArray,
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            jobHandle = addComponentJob.Schedule(this, inputDeps);
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        //}
        /*
        else
        {
            NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetQuery.CalculateEntityCount() , Allocator.TempJob);

            FillArrayEntityWithPositionJob fillArrayEntityWithPositionJob = new FillArrayEntityWithPositionJob
            {
                TargetArray = targetArray
            };
            JobHandle jobHandle = fillArrayEntityWithPositionJob.Schedule(this, inputDeps);
            
            EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
            NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob
            {
                TargetArray = targetArray,
                ClosestTargetEntityArray = closestTargetEntityArray
            };
            jobHandle = findTargetBurstJob.Schedule(this, inputDeps);
            
            AddComponentJob addComponentJob = new AddComponentJob
            {
                ClosestTargetArray = closestTargetEntityArray,
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            jobHandle = addComponentJob.Schedule(this, inputDeps);
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
        */

    }
}