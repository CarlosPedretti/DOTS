using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, LocalTransform.FromPositionRotation(
                authoring.transform.position,
                authoring.transform.rotation
            ));
        }
    }
}

public struct PlayerComponent : IComponentData { }

