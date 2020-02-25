using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
[InternalBufferCapacity(5)]
public struct IntBufferElement : IBufferElementData
{
    public int Value;
}
