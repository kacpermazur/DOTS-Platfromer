using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestBufferSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((DynamicBuffer<IntBufferElement> dynamicBuffer) =>
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                IntBufferElement intBufferElement = dynamicBuffer[i];
                intBufferElement.Value++;
                dynamicBuffer[i] = intBufferElement;
            }
        });
    }
}
