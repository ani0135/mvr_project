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
    private GameObject[] awards; // Array to hold all award GameObjects in the scene
    private ScoreBoardManager scoreBoardManager; // Reference to ScoreBoardManager

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        characterControls = GetComponent<CharacterControls>();
        if (characterControls == null)
        {
            characterControls = gameObject.AddComponent<CharacterControls>();
            Debug.LogWarning("CharacterControls component was missing and has been added at runtime.");
        }
        awards = GameObject.FindGameObjectsWithTag("Award"); // Ensure all awards are tagged as "Award"
        scoreBoardManager = FindObjectOfType<ScoreBoardManager>(); // Get ScoreBoardManager instance in the scene
    }

    public override void OnEpisodeBegin()
    {
        awards = GameObject.FindGameObjectsWithTag("Award"); // Ensure all awards are tagged as "Award"

        if (this.transform.localPosition.y < 0)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            characterControls.LoadCheckPoint(); // Reset to checkpoint
        }

        // Ensure awards array is populated
        if (awards != null)
        {
            foreach (GameObject award in awards)
            {
                if (award != null)
                {
                    award.SetActive(true); // Activate all awards
                }
            }
        }
        else
        {
            Debug.LogError("No awards found. Check if objects are tagged correctly.");
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        GameObject nearestAward = GetNearestAward();
        if (nearestAward != null)
        {
            // Observe nearest award position
            sensor.AddObservation(nearestAward.transform.localPosition);
        }
        else
        {
            // Add dummy data if no award is found (fallback)
            sensor.AddObservation(Vector3.zero);
        }

        // Observe player’s own position and velocity
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        characterControls.Move(controlSignal); // Pass control signal to CharacterControls

        // Reward for reaching the nearest award
        GameObject nearestAward = GetNearestAward();
        if (nearestAward != null)
        {
            float distanceToAward = Vector3.Distance(this.transform.localPosition, nearestAward.transform.localPosition);
            if (distanceToAward < 1.5f) // Threshold for reaching the award
            {
                SetReward(1.0f);
                scoreBoardManager.AddScore(playerIndex, 10); // Add points to player's score when an award is collected
                nearestAward.SetActive(false); // Deactivate the award once collected
            }
        }

        // End the episode if the player falls out of bounds
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal" + playerIndex);
        continuousActionsOut[1] = Input.GetAxis("Vertical" + playerIndex);
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var continuousActionsOut = actionsOut.ContinuousActions;
    //     continuousActionsOut[0] = Input.GetAxis("Horizontal");  // Horizontal axis for movement
    //     continuousActionsOut[1] = Input.GetAxis("Vertical");    // Vertical axis for movement
    // }

    private GameObject GetNearestAward()
    {
        GameObject nearestAward = null;
        float minDistance = float.MaxValue;

        foreach (GameObject award in awards)
        {
            if (award != null && award.activeSelf) // Ensure award is active
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
}
