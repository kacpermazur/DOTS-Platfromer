using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
