using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class PlayerTargetingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref PlayerSingleTarget playerSingleTarget, ref Translation playerTranslation) =>
        {
            Entity closestTargetEntity = Entity.Null;
            float closestDistance = float.MaxValue;

            float3 playerPositon = playerTranslation.Value;
            
            Entities.WithAll<EnemyTag>().ForEach((Entity enemyEntity, ref Translation enemyTranslation) =>
            {
                float targetRange = 12f;
                float enemyDistance = math.distance(playerPositon, enemyTranslation.Value);

                if (enemyDistance < targetRange)
                {
                    if (closestTargetEntity == Entity.Null)
                    {
                        closestTargetEntity = enemyEntity;
                        closestDistance = enemyDistance;
                    }
                    else
                    {
                        if (enemyDistance < targetRange)
                        {
                            closestTargetEntity = enemyEntity;
                            closestDistance = enemyDistance;
                        }
                    }
                }
            });
            playerSingleTarget.TargetEntity = closestTargetEntity;
        });
    }
}
