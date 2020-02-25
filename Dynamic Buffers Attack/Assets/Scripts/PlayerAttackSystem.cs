using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerAttackSystem : ComponentSystem
{
    private float _attackTimer;

    protected override void OnUpdate()
    {
        _attackTimer -= Time.DeltaTime;

        if (_attackTimer <= 0f)
        {
            _attackTimer = 0.6f; // attack rate
            
            float3 playerPosition = float3.zero;
            
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            
            Entities.ForEach((ref PlayerSingleTarget playerSingleTarget) =>
            {
                if (playerSingleTarget.TargetEntity != Entity.Null &&
                    EntityManager.Exists(playerSingleTarget.TargetEntity))
                {
                    ComponentDataFromEntity<Translation> translationComponetData =
                        GetComponentDataFromEntity<Translation>(true);
                    float3 targetPosition = translationComponetData[playerSingleTarget.TargetEntity].Value;

                    Entity attackEntity = EntityManager.Instantiate(GameManager.PfAttackEntity);
                    float3 aimDirection = math.normalize(targetPosition - playerPosition);
                    
                    entityCommandBuffer.SetComponent(attackEntity, new Translation{Value = playerPosition});
                    entityCommandBuffer.SetComponent(attackEntity, new Rotation{Value = quaternion.Euler(0,0,GetAngleFromVector(aimDirection) - math.PI / 2f)});
                    entityCommandBuffer.SetComponent(attackEntity, new Attack{TargetPosition = targetPosition});
                }
            });
            
            entityCommandBuffer.Playback(EntityManager);
        }
    }

    private float GetAngleFromVector(float3 direction)
    {
        direction = math.normalize(direction);
        float n = math.atan2(direction.y, direction.x);
        return n;
    }
}
