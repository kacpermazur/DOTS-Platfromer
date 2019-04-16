using Unity.Entities;
using Unity.Mathematics;

namespace Player.Input
{
    [System.Serializable]
    public struct PlayerInputData : IComponentData
    {
        public float3 MovementDirection;
        public int JumpButtonPressed;
        public int MeleeButtonPressed;
        public int ProjectileButtonPressed;
    }

    public class InputDataComponent : ComponentDataProxy<PlayerInputData>
    {
    }
}