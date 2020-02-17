using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    [SerializeField] private Material unitMat;
    [SerializeField] private Material targetMat;
    [SerializeField] private Mesh quadMesh;

    private EntityManager _entityManager;

    private float spawnTimer;
    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        for (int i = 0; i < 4; i++)
        {
            SpawnUnitEntity();
        }
        

        
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer < 0)
        {
            spawnTimer = 0.1f;
            
            for (int i = 0; i < 5; i++)
            {
                SpawnUnitTarget();
            }
        }
        
        
    }

    private void SpawnUnitEntity()
    {
        SpawnUnitEntity(new float3(Random.Range(-8, 8f), Random.Range(-5, 5f), 0));
    }

    private void SpawnUnitEntity(float3 position)
    {
        Entity entity = _entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Unit)
        );

        SetEntityData(entity, position, quadMesh, unitMat);
        _entityManager.SetComponentData(entity, new Scale {Value = 1.5f});
    }
    
    private void SpawnUnitTarget()
    {
        Entity entity = _entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Target)
        );
        
        SetEntityData(entity, new float3(Random.Range(-8, 8f), Random.Range(-5, 5f), 0), quadMesh, targetMat);
        _entityManager.SetComponentData(entity, new Scale {Value = 0.5f});
    }

    private void SetEntityData(Entity entity, float3 spawnPosition, Mesh mesh, Material material)
    {
        _entityManager.SetSharedComponentData(entity,
            new RenderMesh
            {
                material = material,
                mesh = mesh
            });
        
        _entityManager.SetComponentData(entity,
            new Translation
            {
                Value = spawnPosition
            });
    }
}

public struct Unit : IComponentData { }
public struct Target : IComponentData { }
public struct HasTarget : IComponentData
{
    public Entity TargetEntity;
}

public class HasTargetSytem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref HasTarget hasTarget) =>
        {
            Translation targetTranslation =
                World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(
                    hasTarget.TargetEntity);
            
            Debug.DrawLine(translation.Value, targetTranslation.Value);
        });
    }
}