using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestingJobsOutPut : MonoBehaviour
{
    private void Start()
    {
        NativeArray<int> resultsArray = new NativeArray<int>(1, Allocator.TempJob);

        SimpleJobAdd jobAdd = new SimpleJobAdd
        {
            a = 2,
            b = 3,
            Result = resultsArray
        };
        
        JobHandle jobHandle = jobAdd.Schedule();
        jobHandle.Complete();
        
        Debug.Log(resultsArray[0] + " || " + jobAdd.Result[0]);
        resultsArray.Dispose();
    }
}

public struct SimpleJobAdd : IJob
{
    public int a;
    public int b;
    public NativeArray<int> Result;
    
    public void Execute()
    {
        Result[0] = a + b;
    }
}
