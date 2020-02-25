using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;

public class TestRayCast : MonoBehaviour
{
    
    private Entity RayCast(float3 origin, float3 target)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = origin,
            End = target,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };
        RaycastHit raycastHit = new RaycastHit();

        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            Entity entity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            return entity;
        }
        else
        {
            return Entity.Null;
        }
    }
    

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float rayDistance = 100f;
            Debug.Log(RayCast(ray.origin, ray.direction * rayDistance));
        }
    }
}
