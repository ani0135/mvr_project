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
    // private GameObject[] obstacles; // Array to hold all red obstacle GameObjects
    private ScoreBoardManager scoreBoardManager; // Reference to ScoreBoardManager

    void Start()
    {   
        // rb = GetComponent<Rigidbody>();
        // rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        // characterControls = GetComponent<CharacterControls>();
        // characterControls = GetComponent<CharacterControls>() ?? gameObject.AddComponent<CharacterControls>();
        

        // Initialize arrays with tagged objects
        awards = GameObject.FindGameObjectsWithTag("Award");
        // obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        scoreBoardManager = FindObjectOfType<ScoreBoardManager>();
    }

    public override void OnEpisodeBegin()
    {   
        rb = GetComponent<Rigidbody>();
        // rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        characterControls = GetComponent<CharacterControls>();
        // awards = GameObject.FindGameObjectsWithTag("Award");
        // obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        // Reset player position if it has fallen
        if (this.transform.localPosition.y < 0)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            characterControls.LoadCheckPoint(); // Reset to checkpoint
        }

        // Reactivate all awards if none are active
        bool anyActive = false;
        foreach (GameObject award in awards)
        {
            if (award.activeSelf)
            {
                anyActive = true;
                break;
            }
        }

        if (!anyActive) // If all awards were collected, reactivate them
        {
            foreach (GameObject award in awards)
            {
                if (award != null) award.SetActive(true);
            }
        }
    }

    

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add nearest award position (3 values)
        GameObject nearestAward = GetNearestAward();
        sensor.AddObservation(nearestAward != null ? nearestAward.transform.localPosition : Vector3.zero);
        // Debug.Log(nearestAward.transform.localPosition);
        // Add player’s own position (3 values)
        sensor.AddObservation(this.transform.localPosition);

        // Add player’s velocity (2 values)
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

        // Calculate alignment (1 value)
        // if (nearestAward != null)
        // {
            // // Direction vector to the award
            // Vector3 directionToAward = (nearestAward.transform.localPosition - this.transform.localPosition).normalized;
            
            // // Calculate angle between player’s forward direction and direction to the award
            // float alignment = Vector3.Dot(transform.forward, directionToAward);

            // Add alignment observation
            // sensor.AddObservation(transform.forward.x);
            // sensor.AddObservation(transform.forward.z);
        // }
        // else
        // {
            // If no award is found, add zero as alignment observation
            // sensor.AddObservation(0f);
        // }
    }

    


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = new Vector3(actionBuffers.ContinuousActions[0], 0, actionBuffers.ContinuousActions[1]);
        characterControls.Move(controlSignal);

        // Check for proximity to nearest award
        GameObject nearestAward = GetNearestAward();
        if (nearestAward != null && Vector3.Distance(this.transform.localPosition, nearestAward.transform.localPosition) < 1.5f)
        {
            SetReward(2.0f);
            scoreBoardManager.AddScore(playerIndex, 10); // Increase score
            nearestAward.SetActive(false); // Deactivate collected award
        }

        // Penalty for touching obstacles
        // if (IsTouchingObstacle())
        // {
        //     SetReward(-1.0f);
        //     Debug.Log("negative reward");
        //     scoreBoardManager.AddScore(playerIndex, -1); // Reduce score
        //     EndEpisode();
        // }

        // End episode if the player falls
        if (this.transform.localPosition.y < 0)
        {   
            SetReward(-1.0f);
            Debug.Log("negative reward");
            scoreBoardManager.AddScore(playerIndex, -1); // Reduce score
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Use user input for movement
        continuousActionsOut[0] = Input.GetAxis("Horizontal" + playerIndex); // For horizontal movement (x-axis)
        continuousActionsOut[1] = Input.GetAxis("Vertical" + playerIndex);   // For vertical movement (z-axis)
        
        // Debugging log for heuristic actions
        // Debug.Log("Heuristic action for player " + playerIndex + ": " + continuousActionsOut[0] + ", " + continuousActionsOut[1]);
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var continuousActionsOut = actionsOut.ContinuousActions;
    //     // Use user input for movement
    //     continuousActionsOut[0] = Input.GetAxis("Horizontal"); // For horizontal movement (x-axis)
    //     continuousActionsOut[1] = Input.GetAxis("Vertical");   // For vertical movement (z-axis)
        
    //     // Debugging log for heuristic actions
    //     Debug.Log("Heuristic action for player " + playerIndex + ": " + continuousActionsOut[0] + ", " + continuousActionsOut[1]);
    // }


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

    // private bool IsTouchingObstacle()
    // {
    //     foreach (GameObject obstacle in obstacles)
    //     {
    //         // if (obstacle != null && Vector3.Distance(this.transform.localPosition, obstacle.transform.localPosition) < 2.0f)
    //         // {
    //         //     Debug.Log(Vector3.Distance(this.transform.localPosition, obstacle.transform.localPosition));
    //         //     return true; // Player is close enough to an obstacle
                
    //         // }

    //         if (obstacle != null && Vector2.Distance(new Vector2(this.transform.localPosition.x, this.transform.localPosition.z),
    //         new Vector2(obstacle.transform.localPosition.x, obstacle.transform.localPosition.z))< 1.0f)
    //         {
    //             Debug.Log(Vector2.Distance(new Vector2(this.transform.localPosition.x, this.transform.localPosition.z),
    //         new Vector2(obstacle.transform.localPosition.x, obstacle.transform.localPosition.z)));
    //             return true; // Player is close enough to an obstacle
                
    //         }
    //     }
    //     return false;
    // }
}
