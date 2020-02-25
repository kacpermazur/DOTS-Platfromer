using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;


public struct QuadrantEntity : IComponentData
{
    public Type TypeEnum;
    public enum Type
    {
        Unit,
        Target
    }
}
public struct QuadrantData
{
    public Entity Entity;
    public float3 Position;
    public QuadrantEntity QuadrantEntity;
}
public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> QuadrantMultiHashMap;
    
    private const int QuadrantMultiplier = 1000;
    private const int QuadrantCellSize = 5;
    
    public static int GetPositionHashMapKey(float3 position)
    {
        return (int) (math.floor(position.x / QuadrantCellSize) +
                      (QuadrantMultiplier * math.floor(position.y / QuadrantCellSize)));
    }

    public static void DebugDrawQuadrant(float3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / QuadrantCellSize) * QuadrantCellSize,
            math.floor(position.y / QuadrantCellSize) * QuadrantCellSize);
        
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(1, 0) * QuadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(0, 1) * QuadrantCellSize);
        
        Debug.DrawLine(lowerLeft + new Vector3(1, 0) * QuadrantCellSize , lowerLeft + new Vector3(1, 1) * QuadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(0, 1) * QuadrantCellSize, lowerLeft + new Vector3(1, 1) * QuadrantCellSize);
        
        //Debug.Log(GetPositionHashMapKey(position) + " " + position);
    }

    public static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantHashMap, int hasMapKey)
    {
        QuadrantData quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

        int count = 0;

        if (quadrantHashMap.TryGetFirstValue(hasMapKey, out quadrantData, out nativeMultiHashMapIterator))
        {
            do
            {
                count++;
            } while (quadrantHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }

        return count;
    }
    
    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter QuadrantMultiHashMap;
        
        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEntity)
        {
            int hasMapKey = GetPositionHashMapKey(translation.Value);
            QuadrantMultiHashMap.Add(hasMapKey, new QuadrantData
            {
                Entity = entity,
                Position = translation.Value,
                QuadrantEntity = quadrantEntity
            });
        }
    }

    protected override void OnCreate()
    {
        QuadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
        
    }

    protected override void OnDestroy()
    {
        QuadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));

        QuadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > QuadrantMultiHashMap.Capacity)
        {
            QuadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }
        
        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            QuadrantMultiHashMap = QuadrantMultiHashMap.AsParallelWriter()
        };
        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();
        
        DebugDrawQuadrant(Game.Instance.GetMouseWorldPosition());
        Debug.Log(GetEntityCountInHashMap(QuadrantMultiHashMap, GetPositionHashMapKey(Game.Instance.GetMouseWorldPosition())));
    }
}
