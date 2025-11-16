using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemyDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float3 playerPos = float3.zero;
        bool foundPlayer = false;

        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>())
        {
            playerPos = transform.ValueRO.Position;
            foundPlayer = true;
            break;
        }

        UnityEngine.Debug.Log("Found Player? " + foundPlayer + " pos=" + playerPos);

        foreach (var enemy in SystemAPI.Query<RefRW<EnemyComponent>, RefRO<LocalTransform>>())
        {
            float3 pos = enemy.Item2.ValueRO.Position;

            if (math.distance(pos, playerPos) <= enemy.Item1.ValueRO.DetectionRange)
            {
                enemy.Item1.ValueRW.Mode = EnemyMode.Chase;
                enemy.Item1.ValueRW.TargetPosition = playerPos;
            }
        }
    }
}
