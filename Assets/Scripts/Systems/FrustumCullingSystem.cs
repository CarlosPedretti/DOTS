using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct FrustumCullingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //if (Camera.main == null)
        //    return;

        //var cam = Camera.main;

        //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        //foreach (var (localToWorld, entity) in
        //    SystemAPI.Query<RefRO<LocalToWorld>>()
        //             .WithAll<FrustumCull>()
        //             .WithEntityAccess())
        //{
        //    float3 pos = localToWorld.ValueRO.Position;

        //    bool visible = GeometryUtility.TestPlanesAABB(
        //        planes, new Bounds(pos, new float3(1, 1, 1))
        //    );

        //    state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(entity, visible);
        //}
    }
}
