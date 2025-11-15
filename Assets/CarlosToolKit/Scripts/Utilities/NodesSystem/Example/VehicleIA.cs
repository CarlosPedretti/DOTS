using UnityEngine;
using Utilities;

public class VehicleAI : MonoBehaviour
{
    [SerializeField] private NodeNavigator navigator;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int pathResolution = 20; // Higher = smoother path

    private float timeOnPath = 0f;
    private float pathLength = 1f;
    private Vector3 lastPosition;
    private bool isFollowingPath = true;

    private void Start()
    {
        // Start at the current node's position
        lastPosition = navigator.GetCurrentNode().transform.position;
        transform.position = lastPosition;
        UpdatePathLength();
    }

    private void Update()
    {
        if (!isFollowingPath) return;

        timeOnPath += Time.deltaTime * speed;
        float t = timeOnPath / pathLength;

        Vector3? pos = navigator.GetInterpolatedPosition(t, pathResolution);
        if (pos.HasValue)
        {
            transform.position = pos.Value;
            transform.forward = (pos.Value - lastPosition).normalized;
            lastPosition = pos.Value;
        }

        if (t >= 1f)
        {
            navigator.MoveToNextNode();
            timeOnPath = 0f;
            UpdatePathLength();
        }
    }

    private void UpdatePathLength()
    {
        Vector3 from = navigator.GetCurrentNode().transform.position;
        var path = navigator.GetPathToNextNode(from, pathResolution);

        pathLength = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            pathLength += Vector3.Distance(path[i], path[i + 1]);
        }

        lastPosition = from;
    }
}
