using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnemyMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<EnemyTag>().ForEach((ref Translation translation) =>
        {
            float3 playerPostion = float3.zero;

            float3 moveDirection = math.normalize(playerPostion - translation.Value);

            float movementSpeed = 2f;

            translation.Value += moveDirection * movementSpeed * Time.DeltaTime;
        });
    }
}
