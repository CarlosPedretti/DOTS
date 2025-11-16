using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, enemy) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<EnemyComponent>>())
        {
            float3 pos = transform.ValueRO.Position;
            float3 target = enemy.ValueRO.TargetPosition;

            float3 dir = target - pos;
            float dist = math.length(dir);

            if (dist < 0.1f)
                continue;

            dir = math.normalize(dir);

            quaternion targetRot = quaternion.LookRotationSafe(dir, math.up());

            transform.ValueRW.Rotation = math.slerp(
                transform.ValueRO.Rotation,
                targetRot,
                enemy.ValueRO.RotationSpeed * dt);

            transform.ValueRW.Position += dir * enemy.ValueRO.MoveSpeed * dt;
        }
    }
}
