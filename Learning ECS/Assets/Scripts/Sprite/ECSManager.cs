using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class ECSManager : MonoBehaviour
{

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.Temp);
        
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation)
            //typeof(Scale)
            //typeof(NonUniformScale)
            );

        entityManager.CreateEntity(entityArchetype, entities);

        foreach (var entity in entities)
        {
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = CreateMesh(1f,1f),
                material = _material
            });
            
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3()
            });
        }


        entities.Dispose();
    }

    private Mesh CreateMesh(float width, float height)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
            
        
        vertices[0] = new Vector3(-halfWidth, -halfHeight);
        vertices[1] = new Vector3(-halfWidth, halfHeight);
        vertices[2] = new Vector3(halfWidth, halfHeight);
        vertices[3] = new Vector3(halfWidth, -halfHeight);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;
        
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

}

public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation) =>
        {
            float moveSpeed = 1.0f;

            translation.Value.y += moveSpeed * Time.DeltaTime;
        });
    }
}

public class RotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotation rotation) =>
        {
            float moveSpeed = 1.0f;

            rotation.Value = quaternion.Euler(0,0,math.PI * (float)Time.ElapsedTime);
        });
    }
}

public class ScalerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref NonUniformScale scale) =>
        {

            scale.Value = new float3(2.0f);
        });
    }
}