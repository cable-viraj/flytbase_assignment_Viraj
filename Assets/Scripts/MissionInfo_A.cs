using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionInfo_A : MonoBehaviour
{
    // Waypoints for DroneA (x, y, z)
    public List<Vector3> waypoints = new List<Vector3>
    {
        new Vector3(-5, 0, -5),
        new Vector3(0, 2, 0),
        new Vector3(5, 4, 5),
        new Vector3(10, 6, 0),
        new Vector3(15, 8, -5),
        new Vector3(20, 10, -10),
        new Vector3(25, 12, -5),
        new Vector3(30, 14, 0)
    };

    // Mission time window (seconds since simulation start)
    public float missionStartTime = 10f;
    public float missionEndTime = 20f;

    // Starting point (first waypoint, local to drone object)
    public Vector3 StartPoint => waypoints.Count > 0 ? waypoints[0] : Vector3.zero;

    // Ending point (last waypoint, local to drone object)
    public Vector3 EndPoint => waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : Vector3.zero;

    // Visualize waypoints in purple
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 1f); // Purple
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
