using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class conflict_response : MonoBehaviour
{
    public float currentTime { get; private set; }

    void Start()
    {
        currentTime = 0f;
    }

    void Update()
    {
        currentTime = Time.time;
    }

    public float GetConflictTimestamp()
    {
        return currentTime;
    }

    // Conflict info struct
    public struct ConflictInfo
    {
        public Vector3 location;
        public float time;
        public string dronesInvolved;
    }

    // Accepts the primary drone's mission and other drones' missions
    public string CheckMissionStatus(MonoBehaviour primary, List<MonoBehaviour> others, out List<ConflictInfo> conflictDetails)
    {
        conflictDetails = new List<ConflictInfo>();
        var primaryWaypoints = GetWaypoints(primary);
        var primaryStart = GetMissionStartTime(primary);
        var primaryEnd = GetMissionEndTime(primary);

        foreach (var other in others)
        {
            var otherWaypoints = GetWaypoints(other);
            var otherStart = GetMissionStartTime(other);
            var otherEnd = GetMissionEndTime(other);

            if (primaryWaypoints == null || otherWaypoints == null) continue;

            for (int i = 0; i < primaryWaypoints.Count; i++)
            {
                for (int j = 0; j < otherWaypoints.Count; j++)
                {
                    if (Vector3.Distance(primaryWaypoints[i], otherWaypoints[j]) < 0.1f)
                    {
                        float startOverlap = Mathf.Max(primaryStart, otherStart);
                        float endOverlap = Mathf.Min(primaryEnd, otherEnd);
                        if (startOverlap < endOverlap)
                        {
                            conflictDetails.Add(new ConflictInfo
                            {
                                location = primaryWaypoints[i],
                                time = GetConflictTimestamp(),
                                dronesInvolved = $"{primary.name} & {other.name}"
                            });
                        }
                    }
                }
            }
        }

        // Print status and details to the console
        if (conflictDetails.Count > 0)
        {
            Debug.Log("Mission Status: conflict detected");
            foreach (var conflict in conflictDetails)
            {
                Debug.Log($"Conflict at {conflict.location} between {conflict.dronesInvolved} at time {conflict.time}");
            }
            return "conflict detected";
        }
        else
        {
            Debug.Log("Mission Status: clear");
            return "clear";
        }
    }

    // Helper methods using reflection
    List<Vector3> GetWaypoints(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("waypoints");
        return field?.GetValue(drone) as List<Vector3>;
    }

    float GetMissionStartTime(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("missionStartTime");
        return field != null ? (float)field.GetValue(drone) : 0f;
    }

    float GetMissionEndTime(MonoBehaviour drone)
    {
        var field = drone.GetType().GetField("missionEndTime");
        return field != null ? (float)field.GetValue(drone) : 0f;
    }
}
