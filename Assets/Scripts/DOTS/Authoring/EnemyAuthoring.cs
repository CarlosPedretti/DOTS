using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Random = Unity.Mathematics.Random;

public class EnemyAuthoring : MonoBehaviour
{
    public EnemyMode InitialMode = EnemyMode.Patrol;
    public float MoveSpeed = 5f;
    public float RotationSpeed = 5f;
    public float DetectionRange = 10f;

    public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyComponent
            {
                Mode = authoring.InitialMode,
                MoveSpeed = authoring.MoveSpeed,
                RotationSpeed = authoring.RotationSpeed,
                DetectionRange = authoring.DetectionRange,

                TargetPosition = float3.zero,
                PatrolTimer = 0f,

                Random = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue))
            });
        }
    }
}

public enum EnemyMode { Patrol, Chase }

public struct EnemyComponent : IComponentData
{
    public EnemyMode Mode;

    public float MoveSpeed;
    public float RotationSpeed;
    public float DetectionRange;

    public float3 TargetPosition;
    public float PatrolTimer;

    public Random Random;
}
