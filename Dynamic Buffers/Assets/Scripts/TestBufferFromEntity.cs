using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestBufferFromEntity : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<Tag_B>().ForEach((Entity bEntity) =>
        {
            BufferFromEntity<IntBufferElement> bufferFromEntity = GetBufferFromEntity<IntBufferElement>();

            Entity aEntity = Entity.Null;
            Entities.WithAll<Tag_A>().ForEach((Entity aEntityTemp) => { aEntity = aEntityTemp; });

            DynamicBuffer<IntBufferElement> aEntityDynamicBuffer = bufferFromEntity[aEntity];

            for (int i = 0; i < aEntityDynamicBuffer.Length; i++)
            {
                IntBufferElement intBufferElement = aEntityDynamicBuffer[i];
                intBufferElement.Value++;
                aEntityDynamicBuffer[i] = intBufferElement;
            }
        });
    }
}
