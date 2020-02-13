using Core.System;
using Player.Movement;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Player.Input
{
    [UpdateBefore(typeof(PlayerMovementSystem))]
    public class InputSystem : GameMangerComponentSystem
    {
        private EntityQuery _inputDataGroup;
        
        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _inputDataGroup = GetEntityQuery(typeof(PlayerInputData));;
        }

        protected override void OnUpdate()
        {
            var inputDataReadWrite =
                GetArchetypeChunkComponentType<PlayerInputData>(false);

            NativeArray<ArchetypeChunk> inputDataChunk = _inputDataGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            if (inputDataChunk.Length == 0)
            {
                Debug.Log("test");
                inputDataChunk.Dispose();
                return;
            }

            for (int chunkIndex = 0; chunkIndex < inputDataChunk.Length; chunkIndex++)
            {
                var currentChuck = inputDataChunk[chunkIndex];
                int dataInChunk = currentChuck.Count;
               
                var inputDataArray = currentChuck.GetNativeArray(inputDataReadWrite);

                for (int dataArrayIndex = 0; dataArrayIndex < dataInChunk; dataArrayIndex++)
                {
                    var inputDataSingle = inputDataArray[dataArrayIndex];
                    
                    float horizontalMovement = 0.0f;
                    bool isJumpButtonPressed = false;
                    bool isMeleeButtonPressed = false;
                    bool isProjectileButtonPressed = false;

                    switch (chunkIndex)
                    {
                        case 0:
                        {
                            horizontalMovement = UnityEngine.Input.GetAxisRaw("Horizontal");
                            isJumpButtonPressed = UnityEngine.Input.GetButton("Vertical");
                            isMeleeButtonPressed = UnityEngine.Input.GetButton("Melee");
                            isProjectileButtonPressed = UnityEngine.Input.GetButton("Projectile");
                            break;
                        }
                        default:
                        {
                            Debug.LogError("More than one PlayerInput Entity Found!");
                            break;
                        }
                    }

                    inputDataSingle.MovementDirection.x = horizontalMovement;
                    inputDataSingle.JumpButtonPressed = isJumpButtonPressed ? 1 : 0;
                    inputDataSingle.MeleeButtonPressed = isMeleeButtonPressed ? 1 : 0;
                    inputDataSingle.ProjectileButtonPressed = isProjectileButtonPressed ? 1 : 0;

                    inputDataArray[dataArrayIndex] = inputDataSingle;
                }
            }
            
            inputDataChunk.Dispose();
        }
    }
}