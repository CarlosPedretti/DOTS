using UnityEngine;

namespace Utilities
{
    [AddComponentMenu("Frustum Culling/Frustum Culling Edge")]
    public class FrustumCullingEdge : MonoBehaviour
    {
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}
