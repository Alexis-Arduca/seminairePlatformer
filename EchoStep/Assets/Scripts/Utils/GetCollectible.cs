using System.Collections;
using UnityEngine;
using TMPro;

public class GetCollectible : MonoBehaviour
{
    public int collectibleId;

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameEventsManager.instance.collectibleEvents.OnCollectibleGet(collectibleId);

            Destroy(this.gameObject);
        }
    }
}
