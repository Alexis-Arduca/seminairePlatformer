using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public CollectibleEvents collectibleEvents;
    public PlayerEvents playerEvents;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Game Events Manager in the scene.");
        }
        instance = this;

        // initialize all events
        collectibleEvents = new CollectibleEvents();
        playerEvents = new PlayerEvents();
    }
}