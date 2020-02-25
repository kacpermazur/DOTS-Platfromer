using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private EntityManager _entityManager;
    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        Entity entity = _entityManager.CreateEntity();
        //DynamicBuffer<IntBufferElement> dynamicBuffer = _entityManager.AddBuffer<IntBufferElement>(entity);

        _entityManager.AddBuffer<IntBufferElement>(entity);
        DynamicBuffer<IntBufferElement> dynamicBuffer = _entityManager.GetBuffer<IntBufferElement>(entity);

        dynamicBuffer.Add(new IntBufferElement {Value = 1});
        dynamicBuffer.Add(new IntBufferElement {Value = 2});
        dynamicBuffer.Add(new IntBufferElement {Value = 3});
        
        DynamicBuffer<int> intDynamicBuffer = dynamicBuffer.Reinterpret<int>();
        intDynamicBuffer[1] = 5;


        for (int i = 0; i < intDynamicBuffer.Length; i++)
        {
            intDynamicBuffer[i]++;
        }

        
    }
}
