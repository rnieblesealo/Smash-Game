using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    public List<Transform> targets = new List<Transform>();
    public Vector3 offset;

    [SerializeField] private float smoothTime;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 40f;

    private Vector3 velocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        GetTargets();
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
            return;

        Pan();
        Zoom();
    }

    private void Pan()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(minZoom, maxZoom, GetGreatestDistance() / maxZoom);

        cam.fieldOfView = newZoom;
    }

    void GetTargets()
    {
        // Get transform of all players
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        for (int i = 0; i < players.Length; ++i)
            if (!targets.Contains(players[i].transform))
                targets.Add(players[i].transform);
    }

    float GetGreatestDistance()
    {
        // Use width of bounds to get greatest distance between players
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
            for (int i = 0; i < targets.Count; ++i)
                bounds.Encapsulate(targets[i].position);

        return bounds.size.z; // Bounds are 3D; z axis represents forward so this is what will give the distance
    }

    Vector3 GetCenterPoint()
    {
        // Place in bounds and get center point
        if (targets.Count == 1)
            return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; ++i)
            bounds.Encapsulate(targets[i].position);

        return bounds.center;
    }
}
