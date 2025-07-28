using System.Collections.Generic;
using UnityEngine;

// Main server script for drone conflict detection and visualization
public class Drone_Server : MonoBehaviour
{
    // References to other scripts and prefabs set in the Unity Inspector
    public Conflict_resolution conflictResolver; // Handles conflict resolution logic
    public MissionInfo_A droneA;                // Mission info for Drone A
    public MissionInfo_B droneB;                // Mission info for Drone B
    public MissionInfo_C droneC;                // Mission info for Drone C
    public GameObject markerPrefab;             // Prefab for marking conflict points in the scene

    // Struct to store information about a detected conflict
    public struct ConflictInfo
    {
        public Vector3 location;        // The waypoint (relative position) where conflict occurs
        public float time;              // The time (start of overlap) when conflict occurs
        public string dronesInvolved;   // String label of drones involved (e.g., "A & B")
    }

    // List to store all detected conflicts
    public List<ConflictInfo> conflicts = new List<ConflictInfo>();

    // Unity's Start() method is called once when the scene starts
    void Start()
    {

        // Prepare lists for each drone type (for compatibility with conflictResolver)
        List<MissionInfo_A> dronesA = new List<MissionInfo_A> { droneA };
        List<MissionInfo_B> dronesB = new List<MissionInfo_B> { droneB };
        List<MissionInfo_C> dronesC = new List<MissionInfo_C> { droneC };

        // Call the conflict resolution method (from Conflict_resolution script)
        // This prints suggestions to the console but does not affect detection/visualization
        List<MonoBehaviour> allDrones = new List<MonoBehaviour> { droneA, droneB, droneC };
        conflictResolver.IntelligentResolveConflicts(allDrones);

        // Detect actual conflicts based on current mission info
        DetectConflicts();

        // Print detected conflicts to the Unity Console
        PrintConflictsToConsole();

        // Highlight conflict points and paths in the game view using markers and lines
        HighlightConflictPoints();
    }

    // Detects all pairwise conflicts between drones and stores them in the 'conflicts' list
    void DetectConflicts()
    {
        conflicts.Clear(); // Remove previous results
        CheckConflict(droneA, droneB, "A & B");
        CheckConflict(droneA, droneC, "A & C");
        CheckConflict(droneB, droneC, "B & C");
    }

    // Highlights conflict points and draws lines between them in the game view
    void HighlightConflictPoints()
    {
        if (markerPrefab != null && conflicts.Count > 0)
        {
            // Group conflict points by drone for path visualization
            Dictionary<string, List<Vector3>> droneConflictPaths = new Dictionary<string, List<Vector3>>();

            foreach (var conflict in conflicts)
            {
                string[] drones = conflict.dronesInvolved.Split('&');
                string drone1 = drones[0].Trim();
                MonoBehaviour droneObj = GetDroneByName(drone1);

                if (droneObj != null)
                {
                    // Calculate world position for marker (drone's transform + local waypoint)
                    Vector3 localPoint = droneObj.transform.position + conflict.location;

                    // Instantiate marker prefab at conflict point
                    GameObject marker = Instantiate(markerPrefab, localPoint, Quaternion.identity);
                    marker.transform.localScale = Vector3.one * 0.7f;
                    var renderer = marker.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.red; // Color marker red for visibility
                    }

                    // Add point to path for this drone
                    if (!droneConflictPaths.ContainsKey(drone1))
                        droneConflictPaths[drone1] = new List<Vector3>();
                    droneConflictPaths[drone1].Add(localPoint);
                }
            }

            // Draw lines connecting conflict points for each drone using LineRenderer
            foreach (var kvp in droneConflictPaths)
            {
                List<Vector3> pathPoints = kvp.Value;
                if (pathPoints.Count > 1)
                {
                    GameObject lineObj = new GameObject("ConflictPath_" + kvp.Key);
                    var lineRenderer = lineObj.AddComponent<LineRenderer>();
                    lineRenderer.positionCount = pathPoints.Count;
                    lineRenderer.SetPositions(pathPoints.ToArray());
                    lineRenderer.startWidth = 0.2f;
                    lineRenderer.endWidth = 0.2f;
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;
                }
            }
        }
    }

    // Checks for conflicts between two drones and adds them to the 'conflicts' list
    // Called by DetectConflicts()
    void CheckConflict<T, U>(T info1, U info2, string label)
        where T : MonoBehaviour
        where U : MonoBehaviour
    {
        var waypoints1 = GetWaypoints(info1);
        var waypoints2 = GetWaypoints(info2);
        float startTime1 = GetMissionStartTime(info1);
        float startTime2 = GetMissionStartTime(info2);

        float speed1 = GetDroneSpeed(info1);
        float speed2 = GetDroneSpeed(info2);

        if (waypoints1 == null || waypoints2 == null) return;

        for (int i = 0; i < waypoints1.Count; i++)
        {
            for (int j = 0; j < waypoints2.Count; j++)
            {
                if (Vector3.Distance(waypoints1[i], waypoints2[j]) < 0.1f)
                {
                    // Estimate time at each waypoint using speed
                    float estTime1 = startTime1;
                    for (int k = 1; k <= i; k++)
                        estTime1 += Vector3.Distance(waypoints1[k - 1], waypoints1[k]) / Mathf.Max(speed1, 0.01f);

                    float estTime2 = startTime2;
                    for (int l = 1; l <= j; l++)
                        estTime2 += Vector3.Distance(waypoints2[l - 1], waypoints2[l]) / Mathf.Max(speed2, 0.01f);

                    // Only add conflict if times overlap within 1 second
                    if (Mathf.Abs(estTime1 - estTime2) < 1f)
                    {
                        conflicts.Add(new ConflictInfo
                        {
                            location = waypoints1[i],
                            time = estTime1,
                            dronesInvolved = label
                        });
                    }
                }
            }
        }
    }

    // Helper: Get waypoints list from a drone script using reflection
    List<Vector3> GetWaypoints(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("waypoints");
        return field?.GetValue(drone) as List<Vector3>;
    }

    // Helper: Get mission start time from a drone script using reflection
    float GetMissionStartTime(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("missionStartTime");
        return field != null ? (float)field.GetValue(drone) : 0f;
    }

    // Helper: Get mission end time from a drone script using reflection
    float GetMissionEndTime(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("missionEndTime");
        return field != null ? (float)field.GetValue(drone) : 0f;
    }

    // Helper: Get drone script reference by name (used for conflict reporting)
    MonoBehaviour GetDroneByName(string name)
    {
        if (droneA.name == name) return droneA;
        if (droneB.name == name) return droneB;
        if (droneC.name == name) return droneC;
        return null;
    }

    // Helper: Get speed from FlightLogic script attached to the same GameObject
    float GetDroneSpeed(MonoBehaviour drone)
    {
        Flight flight = drone.GetComponent<Flight>();
        return flight != null ? flight.moveSpeed : 5f;
    }

    // Prints details of all detected conflicts to the Unity Console
    void PrintConflictsToConsole()
    {
        if (conflicts.Count > 0)
        {
            Debug.Log("Mission Status: conflict detected");
            foreach (var conflict in conflicts)
            {
                string[] drones = conflict.dronesInvolved.Split('&');
                string drone1 = drones[0].Trim();
                string drone2 = drones[1]. Trim();

                Debug.Log(
                    $"Conflict between {conflict.dronesInvolved} at waypoint {conflict.location} " +
                    $"at time {conflict.time:F2} seconds."
                );
            }
        }
        // If no conflicts, don't print anything
    }
}