using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class IntBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int[] ValueArray;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        DynamicBuffer<IntBufferElement> dynamicBuffer = dstManager.AddBuffer<IntBufferElement>(entity);

        foreach (var value in ValueArray)
        {
            dynamicBuffer.Add(new IntBufferElement {Value = value});
        }
    }
}
