using Unity.Entities;
using Unity.Mathematics;

namespace Player.Movement
{
    [System.Serializable]
    public struct PlayerMovementData : IComponentData
    {
        public float3 FacingDirection;
        public float Speed;
    }
    
    public class PlayerMovementDataComponent : ComponentDataProxy<PlayerMovementData>
    {

    }
}
