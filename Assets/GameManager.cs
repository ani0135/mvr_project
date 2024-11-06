using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cameraHolderPrefab;
    public int numberOfPlayers = 5;
    public Transform[] spawnPoints;
    private int currentPlayerIndex;
    private GameObject[] players;
    private CameraManager cameraManager;
    private GameObject cameraHolder;

    void Start()
    {
        cameraHolder = Instantiate(cameraHolderPrefab);
        cameraManager = cameraHolder.GetComponent<CameraManager>();

        SpawnPlayers();
        SetActivePlayer(currentPlayerIndex);
    }

    // public Transform[] targets; // Array of target transforms (one for each player)

    // private Transform GetTargetForPlayer(int playerIndex)
    // {
    //     if (targets != null && playerIndex < targets.Length)
    //     {
    //         return targets[playerIndex];
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Target not assigned or out of range for player " + playerIndex);
    //         return null;
    //     }
    // }
    void SpawnPlayers()
{
    players = new GameObject[numberOfPlayers];
    Color[] playerColors = { Color.black, Color.blue, Color.green, Color.yellow, Color.magenta };

    for (int i = 0; i < numberOfPlayers; i++)
    {
        players[i] = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
        players[i].name = "Player " + i;
        players[i].GetComponent<CharacterControls>().SetPlayerIndex(i);
        players[i].GetComponent<CharacterControls>().cam = cameraHolder;

        PlayerAgent agent = players[i].AddComponent<PlayerAgent>();
        agent.playerIndex = i; // Assign player-specific index
        // No specific target is assigned now, each player will compete for any available award

        // Assign unique colors to each player (optional)
        Renderer renderer = players[i].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = playerColors[i % playerColors.Length];
            renderer.material.SetFloat("_Mode", 3);
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
    }
}


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Length; // Increment the index first
            SetActivePlayer(currentPlayerIndex);
        }
    }

    void SetActivePlayer(int index)
    {
        for (int i = 0; i < players.Length; i++)
        {
            Renderer renderer = players[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = (i == index) ? 1f : 0.5f;
                renderer.material.color = color;
            }

            var characterControls = players[i].GetComponent<CharacterControls>();
            var rb = players[i].GetComponent<Rigidbody>(); // Assuming you are using Rigidbody for physics

            // If the player is inactive, stop any movement
            if (i == index)
            {
                characterControls.enabled = true; // Enable controls for active player
                rb.isKinematic = false; // Set to non-kinematic to allow normal physics interactions
            }
            else
            {
                characterControls.enabled = false; // Disable controls for inactive players
                characterControls.StopMovement(); // Stop their movement
                rb.isKinematic = true; // Set to kinematic to prevent physics interactions
            }

        currentPlayerIndex = index;
        cameraManager.SwitchTarget(players[currentPlayerIndex].transform);
    }
    }
}
