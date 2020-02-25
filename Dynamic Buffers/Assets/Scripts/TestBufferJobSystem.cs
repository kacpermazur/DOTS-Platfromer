﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class TestBufferJobSystem : JobComponentSystem
{
    public struct BufferJob  : IJobForEachWithEntity_EB<IntBufferElement>
    {
        public void Execute(Entity entity, int index, DynamicBuffer<IntBufferElement> dynamicBuffer)
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                IntBufferElement intBufferElement = dynamicBuffer[i];
                intBufferElement.Value++;
                dynamicBuffer[i] = intBufferElement;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new BufferJob().Schedule(this, inputDeps);
    }
}
