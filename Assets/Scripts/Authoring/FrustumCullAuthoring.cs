using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class FrustumCullAuthoring : MonoBehaviour
{
    //public MeshRenderer meshRenderer;
    //public Material material;

    public class Baker : Baker<FrustumCullAuthoring>
    {
        public override void Bake(FrustumCullAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);

            //MeshFilter mf = authoring.meshRenderer.GetComponent<MeshFilter>();

            //if (mf == null)
            //{
            //    Debug.LogError("El MeshRenderer no tiene MeshFilter asociado.");
            //    return;
            //}

            //var rma = new RenderMeshArray(
            //    new[] { authoring.material },
            //    new[] { mf.sharedMesh }
            //);

            //AddComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            AddComponent<FrustumCull>(entity);
        }
    }
}

public struct FrustumCull : IComponentData { }
