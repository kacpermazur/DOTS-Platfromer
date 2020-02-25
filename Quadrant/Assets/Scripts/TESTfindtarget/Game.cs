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
    private static Game _game;

    [SerializeField] private Material unitMat;
    [SerializeField] private Material targetMat;
    [SerializeField] private Mesh quadMesh;
    [SerializeField] private Camera newCamera;
    
    [SerializeField] private bool useQuadrantSystem;

    private EntityManager _entityManager;

    public static Game Instance => _game;
    public bool UseQuadrant => useQuadrantSystem;
    
    private float spawnTimer;

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;
        
        Vector3 pos = newCamera.ScreenToWorldPoint(mousePosition);
        pos.z = 0;
        
        return pos;
    }

    private void Awake()
    {
        if (!_game)
            _game = this;
    }

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
            typeof(Unit),
            typeof(QuadrantEntity)
        );

        SetEntityData(entity, position, quadMesh, unitMat);
        _entityManager.SetComponentData(entity, new Scale {Value = 1.5f});
        _entityManager.SetComponentData(entity, new QuadrantEntity {TypeEnum = QuadrantEntity.Type.Unit});
    }
    
    private void SpawnUnitTarget()
    {
        Entity entity = _entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(Scale),
            typeof(Target),
            typeof(QuadrantEntity)
        );
        
        SetEntityData(entity, new float3(Random.Range(-8, 8f), Random.Range(-5, 5f), 0), quadMesh, targetMat);
        _entityManager.SetComponentData(entity, new Scale {Value = 0.5f});
        _entityManager.SetComponentData(entity, new QuadrantEntity {TypeEnum = QuadrantEntity.Type.Target});
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

public class HasTargetSystem : ComponentSystem
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