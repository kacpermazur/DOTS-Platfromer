using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Random = UnityEngine.Random;

public class Testing : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(MoveSpeedComponent)
        );
        
        NativeArray<Entity> entityArray = new NativeArray<Entity>(100000, Allocator.Temp);
        
        entityManager.CreateEntity(entityArchetype, entityArray);

        foreach (var entity in entityArray)
        {
            entityManager.SetComponentData(entity, new LevelComponent {level = Random.Range(10, 20)});
            entityManager.SetComponentData(entity, new MoveSpeedComponent {Value = Random.Range(1f, 2f)});
            entityManager.SetComponentData(entity, new Translation {Value = new float3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0)});

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _mesh,
                material = _material
            });
        }

        entityArray.Dispose();
    }
}
