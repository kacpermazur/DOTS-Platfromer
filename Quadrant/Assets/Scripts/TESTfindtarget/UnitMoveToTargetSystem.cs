using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitMoveToTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity unitEntity, ref HasTarget hasTarget, ref Translation translation) =>
        {
            if (World.DefaultGameObjectInjectionWorld.EntityManager.Exists(hasTarget.TargetEntity))
            {
                Translation targetTranslation =
                    World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(
                        hasTarget.TargetEntity);

                float3 targetDir = math.normalize(targetTranslation.Value - translation.Value);
                float moveSpeed = 5f;

                translation.Value += targetDir * moveSpeed * Time.DeltaTime;

                if (math.distance(translation.Value, targetTranslation.Value) < 0.2f)
                {
                    PostUpdateCommands.DestroyEntity(hasTarget.TargetEntity);
                    PostUpdateCommands.RemoveComponent(unitEntity, typeof(HasTarget));
                }
            }
            else
            {
                PostUpdateCommands.RemoveComponent(unitEntity, typeof(HasTarget));
            }
        });
    }
}
