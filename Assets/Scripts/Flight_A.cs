using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight : MonoBehaviour
{
    [SerializeField]
    public float moveSpeed = 5f;

    public float preStartDuration = 5f;
    public float waitAtStart = 1f;

    // Hardcoded waypoints for this drone
    private List<Vector3> waypoints = new List<Vector3>
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

    // Pause instructions: key = waypoint index, value = pause duration
    public Dictionary<int, float> pauseAtWaypoint = new Dictionary<int, float>();

    private float missionStartTime = 10f;
    private float missionEndTime = 20f;

    void Start()
    {
        Debug.Log($"{gameObject.name} starting with speed: {moveSpeed:F2}");
        StartCoroutine(FlightSequence());
    }

    IEnumerator FlightSequence()
    {
        // Move to start point
        Vector3 startWorldPos = transform.position + waypoints[0];
        float elapsed = 0f;
        Vector3 initialPos = transform.position;
        while (elapsed < preStartDuration)
        {
            transform.position = Vector3.Lerp(initialPos, startWorldPos, elapsed / preStartDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = startWorldPos;
        yield return new WaitForSeconds(waitAtStart);

        // Wait until missionStartTime
        float waitTime = missionStartTime - Time.time;
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);

        // Traverse waypoints
        for (int i = 1; i < waypoints.Count; i++)
        {
            Vector3 targetPos = transform.position + (waypoints[i] - waypoints[i - 1]);
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;

            // Pause if instructed
            if (pauseAtWaypoint.ContainsKey(i))
            {
                float pauseDuration = pauseAtWaypoint[i];
                if (pauseDuration > 0f)
                {
                    Debug.Log($"{gameObject.name} pausing at waypoint {i} for {pauseDuration:F2}s.");
                    yield return new WaitForSeconds(pauseDuration);
                }
            }
        }
        // Freeze at end point
        yield break;
    }
}
