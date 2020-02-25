using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AttackMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity attackEntity, ref Translation attackTranslation, ref Attack attack) =>
        {
            float3 moveTowardsTargetDirection = math.normalize(attack.TargetPosition - attackTranslation.Value);
            float moveSpeed = 20f;

            attackTranslation.Value += moveTowardsTargetDirection * moveSpeed * Time.DeltaTime;

            float3 attackTranslationValue = attackTranslation.Value;
            
            Entities.ForEach((Entity enemyEntity, ref Translation enemyTranslation, ref EnemyHealth enemyHealth) =>
            {
                float attackDistance = 1f;

                if (math.distance(attackTranslationValue, enemyTranslation.Value) < attackDistance)
                {
                    enemyHealth.Value--;
                    PostUpdateCommands.DestroyEntity(attackEntity);

                    if (enemyHealth.Value <= 0)
                    {
                        PostUpdateCommands.DestroyEntity(enemyEntity);
                    }
                }
            });

            float distanceAwayUntillDestory = 1f;
            if (math.distance(attackTranslationValue, attack.TargetPosition) < distanceAwayUntillDestory)
            {
                PostUpdateCommands.DestroyEntity(attackEntity);
            }
            
        });
    }
}
