using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    private Rigidbody rb;
    private CharacterControls characterControls;
    public int playerIndex;
    private GameObject[] awards; // Array to hold all green award GameObjects
    private GameObject[] obstacles; // Array to hold all red obstacle GameObjects
    private ScoreBoardManager scoreBoardManager; // Reference to ScoreBoardManager

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        // rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        characterControls = GetComponent<CharacterControls>();
        // characterControls = GetComponent<CharacterControls>() ?? gameObject.AddComponent<CharacterControls>();

        // Initialize arrays with tagged objects
        awards = GameObject.FindGameObjectsWithTag("Award");
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        scoreBoardManager = FindObjectOfType<ScoreBoardManager>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset player position if it has fallen
        if (this.transform.localPosition.y < 0)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            characterControls.LoadCheckPoint(); // Reset to checkpoint
        }

        // Reactivate all awards for collection
        if (awards != null)
        {
            foreach (GameObject award in awards)
            {
                if (award != null) award.SetActive(true);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        // Add nearest award position
        GameObject nearestAward = GetNearestAward();
        if (nearestAward != null)
        {
            sensor.AddObservation(nearestAward.transform.localPosition); // 3 values for position (x, y, z)
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // Dummy data if no award is found
        }

        // Add player’s own position (3 values for position)
        sensor.AddObservation(this.transform.localPosition);

        // Add player's velocity (2 values for x and z velocity)
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
        Debug.Log("Observations for player " + playerIndex + ": " + string.Join(", ", nearestAward.transform.localPosition, this.transform.localPosition, rb.velocity.x, rb.velocity.z));


        // Add any additional relevant observations
        // You may also add other information, like if the player is near obstacles, etc.
        // sensor.AddObservation(this.isNearObstacle()); // Example additional observation, modify accordingly
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = new Vector3(actionBuffers.ContinuousActions[0], 0, actionBuffers.ContinuousActions[1]);
        characterControls.Move(controlSignal);

        // Check for proximity to nearest award
        GameObject nearestAward = GetNearestAward();
        if (nearestAward != null && Vector3.Distance(this.transform.localPosition, nearestAward.transform.localPosition) < 1.0f)
        {
            SetReward(1.0f);
            scoreBoardManager.AddScore(playerIndex, 10); // Increase score
            nearestAward.SetActive(false); // Deactivate collected award
        }

        // Penalty for touching obstacles
        if (IsTouchingObstacle())
        {
            SetReward(-1.0f);
            scoreBoardManager.AddScore(playerIndex, -5); // Reduce score
        }

        // End episode if the player falls
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var continuousActionsOut = actionsOut.ContinuousActions;
    //     // Use user input for movement
    //     continuousActionsOut[0] = Input.GetAxis("Horizontal" + playerIndex); // For horizontal movement (x-axis)
    //     continuousActionsOut[1] = Input.GetAxis("Vertical" + playerIndex);   // For vertical movement (z-axis)
        
    //     // Debugging log for heuristic actions
    //     Debug.Log("Heuristic action for player " + playerIndex + ": " + continuousActionsOut[0] + ", " + continuousActionsOut[1]);
    // }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Use user input for movement
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // For horizontal movement (x-axis)
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // For vertical movement (z-axis)
        
        // Debugging log for heuristic actions
        Debug.Log("Heuristic action for player " + playerIndex + ": " + continuousActionsOut[0] + ", " + continuousActionsOut[1]);
    }


    private GameObject GetNearestAward()
    {
        GameObject nearestAward = null;
        float minDistance = float.MaxValue;

        foreach (GameObject award in awards)
        {
            if (award != null && award.activeSelf)
            {
                float distance = Vector3.Distance(this.transform.localPosition, award.transform.localPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestAward = award;
                }
            }
        }
        return nearestAward;
    }

    private bool IsTouchingObstacle()
    {
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle != null && Vector3.Distance(this.transform.localPosition, obstacle.transform.localPosition) < 1.0f)
            {
                return true; // Player is close enough to an obstacle
            }
        }
        return false;
    }
}
