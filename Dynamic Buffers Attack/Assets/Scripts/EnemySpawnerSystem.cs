using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawnerSystem : ComponentSystem
{
    private Entity _pfEnemyEntity;
    private float _enemySpawnTimer;
    private Unity.Mathematics.Random _random;

    protected override void OnCreate()
    {
        _random = new Unity.Mathematics.Random(50);
    }

    protected override void OnUpdate()
    {
        _enemySpawnTimer -= Time.DeltaTime;

        if (_enemySpawnTimer <= 0f)
        {
            _enemySpawnTimer = 0.3f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Entity enemyEntity = EntityManager.Instantiate(GameManager.PfEnemyEntity);
        EntityManager.SetComponentData(enemyEntity,
            new Translation {Value = GetRandomDirection() * _random.NextFloat(12f, 15f)});
    }

    private float3 GetRandomDirection()
    {
        float3 direction = new float3(_random.NextFloat(-1f, 1), _random.NextFloat(-1f, 1), 0);
        return math.normalize(direction);
    }
}
