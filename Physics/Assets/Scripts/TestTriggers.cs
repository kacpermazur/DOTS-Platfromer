using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class TestTriggers : JobComponentSystem
{
    [BurstCompile]
    struct TriggerJob : ITriggerEventsJob
    {
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityEntities;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            if (PhysicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityA))
            {
                PhysicsVelocity physicsVelocity = PhysicsVelocityEntities[triggerEvent.Entities.EntityA];
                physicsVelocity.Linear.y = 5f;
                PhysicsVelocityEntities[triggerEvent.Entities.EntityA] = physicsVelocity;
            }
            
            if (PhysicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityB))
            {
                PhysicsVelocity physicsVelocity = PhysicsVelocityEntities[triggerEvent.Entities.EntityB];
                physicsVelocity.Linear.y = 5f;
                PhysicsVelocityEntities[triggerEvent.Entities.EntityB] = physicsVelocity;
            }
        }
    }

    private BuildPhysicsWorld _buildPhysicsWorld;
    private StepPhysicsWorld _stepPhysicsWorld;
    
    protected override void OnCreate()
    {
        _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        TriggerJob triggerJob = new TriggerJob
        {
            PhysicsVelocityEntities = GetComponentDataFromEntity<PhysicsVelocity>()
        };

        return triggerJob.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
    }
}
