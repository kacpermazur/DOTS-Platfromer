using Core.System;
using Player.Input;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Player.Movement
{
    public class PlayerMovementSystem : GameMangerJobComponentSystem
    {
        private EntityQuery _playerMovementDataGroup;

        [BurstCompile]
        public struct MovementJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<ArchetypeChunk> chunks;

            [ReadOnly] public ArchetypeChunkComponentType<PlayerInputData> PlayerInputDataReadOnly;
            [ReadOnly] public ArchetypeChunkComponentType<PlayerMovementData> PlayerMovementDataReadOnly;
            public ArchetypeChunkComponentType<Translation> PlayerTransformReadWrite;

            public float DeltaTime;
            
            public void Execute(int chunkIndex)
            {
                var currentChuck = chunks[chunkIndex];
                int dataInChunk = currentChuck.Count;

                var playerInputDataArray = currentChuck.GetNativeArray(PlayerInputDataReadOnly);
                var playerMovementDataArray = currentChuck.GetNativeArray(PlayerMovementDataReadOnly);
                var playerTransformArray = currentChuck.GetNativeArray(PlayerTransformReadWrite);

                for (int dataArrayIndex = 0; dataArrayIndex < dataInChunk; dataArrayIndex++)
                {
                    var playerInputData = playerInputDataArray[dataArrayIndex];
                    var playerMovementData = playerMovementDataArray[dataArrayIndex];
                    var playerTransform = playerTransformArray[dataArrayIndex];

                    float3 fowardVector = playerMovementData.FacingDirection * playerInputData.MovementDirection.x;
                    playerTransform.Value += fowardVector * playerMovementData.Speed * DeltaTime;
                }
            }
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _playerMovementDataGroup = GetEntityQuery(new EntityQueryDesc
            {
                Any = new ComponentType[] {},
                None = new ComponentType[] {},
                All =  new ComponentType[]
                {
                    typeof(PlayerInputData),
                    typeof(PlayerMovementData),
                    typeof(LocalToWorldProxy),
                },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerInputDataReadOnly = GetArchetypeChunkComponentType<PlayerInputData>(true);
            var playerMovementDataReadOnly = GetArchetypeChunkComponentType<PlayerMovementData>(true);
            var playerTransformReadWrite = GetArchetypeChunkComponentType<Translation>(false);

            JobHandle chunkArrayHandle;
            var playerMovementDataChunk =
                _playerMovementDataGroup.CreateArchetypeChunkArray(Allocator.TempJob, out chunkArrayHandle);

            if (playerMovementDataChunk.Length == 0)
            {
                chunkArrayHandle.Complete();
                playerMovementDataChunk.Dispose();
                return inputDeps;
            }

            var moveJobDependency = JobHandle.CombineDependencies(inputDeps, chunkArrayHandle);
            
            var movementJob = new MovementJob
            {
                chunks = playerMovementDataChunk,
                PlayerInputDataReadOnly = playerInputDataReadOnly,
                PlayerMovementDataReadOnly = playerMovementDataReadOnly,
                PlayerTransformReadWrite = playerTransformReadWrite,
                DeltaTime = Time.deltaTime
            };

            return movementJob.Schedule(playerMovementDataChunk.Length, 10, moveJobDependency);
        }
    }
}