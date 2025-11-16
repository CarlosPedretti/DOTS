using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemyPatrolSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (enemy, transform) in
            SystemAPI.Query<RefRW<EnemyComponent>, RefRO<LocalTransform>>()
            .WithAll<EnemyComponent>())
        {
            if (enemy.ValueRO.Mode != EnemyMode.Patrol)
                continue;

            enemy.ValueRW.PatrolTimer -= dt;

            if (enemy.ValueRW.PatrolTimer <= 0f)
            {
                float3 pos = transform.ValueRO.Position;

                float3 offset = enemy.ValueRW.Random.NextFloat3(
                    new float3(-6f, 0f, -6f),
                    new float3(6f, 0f, 6f));

                enemy.ValueRW.TargetPosition = pos + offset;

                enemy.ValueRW.PatrolTimer = 2f;
            }
        }
    }
}
