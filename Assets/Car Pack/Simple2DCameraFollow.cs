using UnityEngine;

// Attach this script to your Main Camera
[RequireComponent(typeof(Camera))]
public class Simple2DMultiTargetCamera : MonoBehaviour
{
    [Header("🎯 Targets to Follow (Add Ball, Car1, Car2, etc)")]
    public Transform[] targets;

    [Header("🎥 Camera Settings")]
    public float followSpeed = 5f;
    public float zOffset = -10f;

    [Header("🔍 Zoom Settings")]
    public float minZoom = 15f;       // Minimum orthographic size (closer = more zoom)
    public float maxZoom = 50f;       // Maximum orthographic size (farther = zoomed out)
    public float zoomLimiter = 30f;   // Lower value = more zoom sensitivity
    public float zoomSpeed = 5f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (targets == null || targets.Length == 0) return;

        Vector3 center = GetCenterPoint();
        Vector3 newPos = new Vector3(center.x, center.y, zOffset);
        transform.position = Vector3.Lerp(transform.position, newPos, followSpeed * Time.deltaTime);

        // Zoom based on distance between targets
        float newZoom = Mathf.Lerp(minZoom, maxZoom, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime * zoomSpeed);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Length == 1)
            return targets[0].position;

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach (Transform t in targets)
        {
            if (t != null)
                bounds.Encapsulate(t.position);
        }

        return bounds.center;
    }

    float GetGreatestDistance()
    {
        if (targets.Length < 2)
            return 0f;

        float maxDist = 0f;
        for (int i = 0; i < targets.Length; i++)
        {
            for (int j = i + 1; j < targets.Length; j++)
            {
                if (targets[i] != null && targets[j] != null)
                {
                    float dist = Vector2.Distance(targets[i].position, targets[j].position);
                    if (dist > maxDist)
                        maxDist = dist;
                }
            }
        }
        return maxDist;
    }

    // Optional: Visualize in Scene View
    void OnDrawGizmos()
    {
        if (targets == null) return;

        Gizmos.color = Color.red;
        foreach (var t in targets)
        {
            if (t != null)
                Gizmos.DrawSphere(t.position, 0.3f);
        }
    }
}
