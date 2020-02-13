using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class TestingJobs : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfUnit;
    private List<Unit> unitList;
    
    public class Unit
    {
        public Transform Transform;
        public float MoveY;
    }

    private void Start()
    {
        unitList = new List<Unit>();
        for (int i = 0; i < 1000; i++)
        {
            Transform unitTransform = Instantiate(pfUnit,
                new Vector3(Random.Range(-8f,8f), Random.Range(-8f,8f), Random.Range(-8f,8f)), Quaternion.identity);
            
            unitList.Add(new Unit
            {
                Transform = unitTransform,
                MoveY = Random.Range(1f, 2f)
            });
        }
    }

    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs)
        {
            //NativeArray<float3> positionArray = new NativeArray<float3>(unitList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(unitList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(unitList.Count);

            for (int i = 0; i < unitList.Count; i++)
            {
                //positionArray[i] = unitList[i].Transform.position;
                moveYArray[i] = unitList[i].MoveY;
                transformAccessArray.Add(unitList[i].Transform);
            }
            /*
            MoveJob moveJob = new MoveJob
            {
                PositionArray = positionArray,
                MoveYArray = moveYArray,
                DeltaTime = Time.deltaTime,
                
            };
            */
            
            MoveTransformJob moveTransformJob = new MoveTransformJob
            {
                MoveYArray = moveYArray,
                DeltaTime = Time.deltaTime,
            };
            

            JobHandle jobs = moveTransformJob.Schedule(transformAccessArray);
            jobs.Complete();

            for (int i = 0; i < unitList.Count; i++)
            {
                //unitList[i].Transform.position = positionArray[i];
                unitList[i].MoveY = moveYArray[i];
            }

            //positionArray.Dispose();
            moveYArray.Dispose();
            transformAccessArray.Dispose();
        }
        else
        {
            foreach (Unit duck in unitList)
            {
                duck.Transform.position += new Vector3(0, duck.MoveY * Time.deltaTime);
                if (duck.Transform.position.y > 5f)
                {
                    duck.MoveY = -math.abs(duck.MoveY);
                }

                if (duck.Transform.position.y < -5f)
                {
                    duck.MoveY = +math.abs(duck.MoveY);
                }
            }
            
        }

        /*
        if (useJobs)
        {
            NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);
            
            for (int i = 0; i < 10; i++)
            {
                JobHandle job = SomeTaskJob();

                jobHandles.Add(job);
            }
            
            JobHandle.CompleteAll(jobHandles);
            jobHandles.Dispose();
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                SomeTask();
            }
            
        }
        */
        
        Debug.Log((Time.realtimeSinceStartup - startTime) * 1000f + "ms");
    }

    private void SomeTask()
    {
        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle SomeTaskJob()
    {
        ExampleJob job = new ExampleJob();
        return job.Schedule();
    }
}

[BurstCompile]
public struct ExampleJob : IJob
{

    public void Execute()
    {
        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct MoveJob : IJobParallelFor
{
    public NativeArray<float3> PositionArray;
    public NativeArray<float> MoveYArray;
    [ReadOnly] public float DeltaTime;
    
    public void Execute(int index)
    {
        PositionArray[index] += new float3(0, MoveYArray[index] * DeltaTime, 0f);
        if (PositionArray[index].y > 5f)
        {
            MoveYArray[index] = -math.abs(MoveYArray[index]);
        }
        if (PositionArray[index].y < -5f)
        {
            MoveYArray[index] = +math.abs(MoveYArray[index]);
        }
    }
}

[BurstCompile]
public struct MoveTransformJob : IJobParallelForTransform
{
    public NativeArray<float> MoveYArray;
    [ReadOnly] public float DeltaTime;
    
    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0, MoveYArray[index] * DeltaTime, 0f);
        
        if (transform.position.y > 5f)
        {
            MoveYArray[index] = -math.abs(MoveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            MoveYArray[index] = +math.abs(MoveYArray[index]);
        }
    }
}