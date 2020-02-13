using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeedComponent) =>
        {
            translation.Value.y += moveSpeedComponent.Value * Time.DeltaTime;

            if (translation.Value.y > 5f)
            {
                moveSpeedComponent.Value = -math.abs(moveSpeedComponent.Value);
            }
            if (translation.Value.y < -5f)
            {
                moveSpeedComponent.Value = +math.abs(moveSpeedComponent.Value);
            }
        });
    }
}
