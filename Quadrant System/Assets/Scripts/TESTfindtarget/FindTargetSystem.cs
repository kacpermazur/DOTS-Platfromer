using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/*
public class FindTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<HasTarget>().WithAll<Unit>().ForEach((Entity entity, ref Translation unitTranslation )=>
        {
            Entity closestTarget = Entity.Null;
            
            float3 unitPosition = unitTranslation.Value;
            float3 closestTargetPosition = float3.zero;
            
            
            Entities.WithAll<Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) =>
            {
                if (closestTarget == Entity.Null)
                {
                    closestTarget = targetEntity;
                    closestTargetPosition = targetTranslation.Value;
                }
                else
                {
                    if (math.distance(unitPosition, targetTranslation.Value) < math.distance(unitPosition, closestTargetPosition))
                    {
                        closestTarget = targetEntity;
                        closestTargetPosition = targetTranslation.Value;
                    }
                }
            });

            if (closestTarget != Entity.Null)
            {

                PostUpdateCommands.AddComponent(entity, new HasTarget
                {
                    TargetEntity = closestTarget
                });
            }
            
        });
    }
}
*/

public class FindTargetJobSystem : JobComponentSystem
{
    private struct EntityWithPosition
    {
        public Entity Entity;
        public float3 Position;
    }
    /*
    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    private struct FindTargetJob : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            Entity closestTarget = Entity.Null;
            
            float3 unitPosition = translation.Value;
            float3 closestTargetPosition = float3.zero;

            for (int i = 0; i < targetArray.Length; i++)
            {
                EntityWithPosition targetEntityWithPosition = targetArray[i];
                
                if (closestTarget == Entity.Null)
                {
                    closestTarget = targetEntityWithPosition.Entity;
                    closestTargetPosition = targetEntityWithPosition.Position;
                }
                else
                {
                    if (math.distance(unitPosition, targetEntityWithPosition.Position) < math.distance(unitPosition, closestTargetPosition))
                    {
                        closestTarget = targetEntityWithPosition.Entity;
                        closestTargetPosition = targetEntityWithPosition.Position;
                    }
                }
            }

            if (closestTarget != Entity.Null)
            {

                EntityCommandBuffer.AddComponent(index, entity, new HasTarget
                {
                    TargetEntity = closestTarget
                });
            }
        }
    }
    */
    
    [RequireComponentTag(typeof(Unit))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation>
    {
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public NativeArray<Entity> ClosestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            Entity closestTarget = Entity.Null;
            
            float3 unitPosition = translation.Value;
            float3 closestTargetPosition = float3.zero;

            for (int i = 0; i < targetArray.Length; i++)
            {
                EntityWithPosition targetEntityWithPosition = targetArray[i];
                
                if (closestTarget == Entity.Null)
                {
                    closestTarget = targetEntityWithPosition.Entity;
                    closestTargetPosition = targetEntityWithPosition.Position;
                }
                else
                {
                    if (math.distance(unitPosition, targetEntityWithPosition.Position) < math.distance(unitPosition, closestTargetPosition))
                    {
                        closestTarget = targetEntityWithPosition.Entity;
                        closestTargetPosition = targetEntityWithPosition.Position;
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

    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);


        NativeArray<EntityWithPosition> targetArray =
            new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);

        for (int i = 0; i < targetArray.Length; i++)
        {
            targetArray[i] = new EntityWithPosition
            {
                Entity = targetEntityArray[i],
                Position = targetTranslationArray[i].Value
            };
        }

        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();

        EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
        NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);
        
        /*
        FindTargetJob findTargetJob = new FindTargetJob
        {
            targetArray = targetArray,
            EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        */
        
        FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob
        {
            targetArray = targetArray,
            ClosestTargetEntityArray = closestTargetEntityArray
            
        };

        JobHandle jobHandle = findTargetBurstJob.Schedule(this, inputDeps);

        AddComponentJob addComponentJob = new AddComponentJob
        {
            ClosestTargetArray = closestTargetEntityArray,
            EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        jobHandle = addComponentJob.Schedule(this, jobHandle);
        
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}