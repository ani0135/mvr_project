using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CameraManager cameraManager; // Reference to the CameraManager

    public void SwitchToPlayer(Transform newTarget)
    {
        cameraManager.SwitchTarget(newTarget);
    }
}
