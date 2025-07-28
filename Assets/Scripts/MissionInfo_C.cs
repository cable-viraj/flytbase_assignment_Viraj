using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionInfo_C : MonoBehaviour
{
    // Waypoints for DroneC (x, y, z)
    public List<Vector3> waypoints = new List<Vector3>
    {
        new Vector3(-10, 4, -10),
        new Vector3(0, 6, -5),
        new Vector3(5, 8, 5),     // Intersection with A & B
        new Vector3(10, 10, 0),   // Intersection with A & B
        new Vector3(15, 12, 10),
        new Vector3(20, 14, 15),
        new Vector3(25, 16, 5),
        new Vector3(30, 18, -5)
    };

    // Mission time window (overlaps with A and B)
    public float missionStartTime = 10f;
    public float missionEndTime = 20f;

    // Starting point (first waypoint, local to drone object)
    public Vector3 StartPoint => waypoints.Count > 0 ? waypoints[0] : Vector3.zero;

    // Ending point (last waypoint, local to drone object)
    public Vector3 EndPoint => waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : Vector3.zero;

    // Visualize waypoints in green
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.DrawSphere(transform.position + waypoints[i], 0.3f);
            if (i < waypoints.Count - 1)
            {
                Gizmos.DrawLine(transform.position + waypoints[i], transform.position + waypoints[i + 1]);
            }
        }
        // Highlight start and end points
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + StartPoint, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + EndPoint, 0.5f);
    }
}
