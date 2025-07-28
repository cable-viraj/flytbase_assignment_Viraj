using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conflict_resolution : MonoBehaviour
{
    public void ResolveConflicts(List<MissionInfo_A> dronesA, List<MissionInfo_B> dronesB, List<MissionInfo_C> dronesC)
    {
        List<MonoBehaviour> allDrones = new List<MonoBehaviour>();
        allDrones.AddRange(dronesA);
        allDrones.AddRange(dronesB);
        allDrones.AddRange(dronesC);

        float safeInterval = 10f; // Minimum time gap between drones at conflict points

        // Store conflicts
        List<(MonoBehaviour, MonoBehaviour)> conflicts = new List<(MonoBehaviour, MonoBehaviour)>();

        // Check for conflicts using speed and time
        for (int i = 0; i < allDrones.Count; i++)
        {
            for (int j = i + 1; j < allDrones.Count; j++)
            {
                MonoBehaviour drone1 = allDrones[i];
                MonoBehaviour drone2 = allDrones[j];

                float speed1 = GetDroneSpeed(drone1);
                float speed2 = GetDroneSpeed(drone2);

                if (HasConflictWithSpeed(drone1, drone2, speed1, speed2))
                {
                    conflicts.Add((drone1, drone2));
                }
            }
        }

        if (conflicts.Count == 0)
        {
            Debug.Log("All drone missions are good to go! No conflicts detected.");
            return;
        }

        Debug.Log("=== WARNING: Drone Conflict Detected ===");
        foreach (var pair in conflicts)
        {
            MonoBehaviour drone1 = pair.Item1;
            MonoBehaviour drone2 = pair.Item2;
            Debug.Log($"Conflict detected between {drone1.name} and {drone2.name}.");

            // Option 1: Suggest time adjustment
            Debug.Log($"Option 1: Adjust start time of {drone2.name} by at least {safeInterval} seconds after {drone1.name}.");

            // Option 2: Suggest reroute (simple offset)
            Debug.Log($"Option 2: Reroute {drone2.name} waypoints by offsetting them (e.g., +2,0,+2) to avoid spatial conflict.");
        }
    }

    // Checks for spatial and temporal conflict using speed
    bool HasConflictWithSpeed(MonoBehaviour drone1, MonoBehaviour drone2, float speed1, float speed2)
    {
        var waypoints1 = GetWaypoints(drone1);
        var waypoints2 = GetWaypoints(drone2);
        float start1 = GetMissionStartTime(drone1);
        float start2 = GetMissionStartTime(drone2);

        // Calculate time at each waypoint using speed
        float time1 = start1;
        for (int i = 1; i < waypoints1.Count; i++)
        {
            float dist = Vector3.Distance(waypoints1[i - 1], waypoints1[i]);
            time1 += dist / Mathf.Max(speed1, 0.01f); // Avoid division by zero
        }

        float time2 = start2;
        for (int j = 1; j < waypoints2.Count; j++)
        {
            float dist = Vector3.Distance(waypoints2[j - 1], waypoints2[j]);
            time2 += dist / Mathf.Max(speed2, 0.01f);
        }

        // Check for spatial conflict at any waypoint
        for (int i = 0; i < waypoints1.Count; i++)
        {
            for (int j = 0; j < waypoints2.Count; j++)
            {
                if (Vector3.Distance(waypoints1[i], waypoints2[j]) < 0.1f)
                {
                    // If their estimated times at the conflict waypoint overlap within 1 second
                    float estTime1 = start1;
                    for (int k = 1; k <= i; k++)
                        estTime1 += Vector3.Distance(waypoints1[k - 1], waypoints1[k]) / Mathf.Max(speed1, 0.01f);

                    float estTime2 = start2;
                    for (int l = 1; l <= j; l++)
                        estTime2 += Vector3.Distance(waypoints2[l - 1], waypoints2[l]) / Mathf.Max(speed2, 0.01f);

                    if (Mathf.Abs(estTime1 - estTime2) < 1f)
                        return true;
                }
            }
        }
        return false;
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

    // Helper: Get speed from FlightLogic script attached to the same GameObject
    float GetDroneSpeed(MonoBehaviour drone)
    {
        Flight flight = drone.GetComponent<Flight>();
        return flight != null ? flight.moveSpeed : 5f; // Default to 5 if not found
    }

    public void IntelligentResolveConflicts(List<MonoBehaviour> allDrones)
    {
        float buffer = 1f; // Minimum buffer time in seconds

        // Identify drones
        MonoBehaviour droneA = allDrones.Find(d => d.name.Contains("A"));
        MonoBehaviour droneB = allDrones.Find(d => d.name.Contains("B"));
        MonoBehaviour droneC = allDrones.Find(d => d.name.Contains("C"));

        var waypointsA = GetWaypoints(droneA);
        var waypointsB = GetWaypoints(droneB);
        var waypointsC = GetWaypoints(droneC);

        float startA = GetMissionStartTime(droneA);
        float startB = GetMissionStartTime(droneB);
        float startC = GetMissionStartTime(droneC);

        Flight flightA = droneA.GetComponent<Flight>();
        Flight flightB = droneB.GetComponent<Flight>();
        Flight flightC = droneC.GetComponent<Flight>();

        // For each shared waypoint, calculate buffer and set pause
        for (int i = 0; i < waypointsA.Count; i++)
        {
            int idxB = waypointsB.FindIndex(w => Vector3.Distance(w, waypointsA[i]) < 0.1f);
            int idxC = waypointsC.FindIndex(w => Vector3.Distance(w, waypointsA[i]) < 0.1f);

            if (idxB != -1)
            {
                float timeA = startA;
                for (int k = 1; k <= i; k++)
                    timeA += Vector3.Distance(waypointsA[k - 1], waypointsA[k]) / Mathf.Max(flightA.moveSpeed, 0.01f);

                float timeB = startB;
                for (int k = 1; k <= idxB; k++)
                    timeB += Vector3.Distance(waypointsB[k - 1], waypointsB[k]) / Mathf.Max(flightB.moveSpeed, 0.01f);

                if (Mathf.Abs(timeA - timeB) < buffer)
                {
                    float pauseB = (timeA + buffer) - timeB;
                    if (pauseB > 0 && idxB > 0)
                    {
                        flightB.pauseAtWaypoint[idxB - 1] = pauseB;
                        Debug.Log($"B pauses for {pauseB:F2}s at waypoint {idxB - 1} to avoid conflict with A.");
                    }
                }
            }

            if (idxC != -1)
            {
                float timeA = startA;
                for (int k = 1; k <= i; k++)
                    timeA += Vector3.Distance(waypointsA[k - 1], waypointsA[k]) / Mathf.Max(flightA.moveSpeed, 0.01f);

                float timeC = startC;
                for (int k = 1; k <= idxC; k++)
                    timeC += Vector3.Distance(waypointsC[k - 1], waypointsC[k]) / Mathf.Max(flightC.moveSpeed, 0.01f);

                if (Mathf.Abs(timeA - timeC) < buffer)
                {
                    float pauseC = (timeA + buffer) - timeC;
                    if (pauseC > 0 && idxC > 0)
                    {
                        flightC.pauseAtWaypoint[idxC - 1] = pauseC;
                        Debug.Log($"C pauses for {pauseC:F2}s at waypoint {idxC - 1} to avoid conflict with A.");
                    }
                }
            }
        }
    }
}
