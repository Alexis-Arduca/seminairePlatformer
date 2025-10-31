using System.Collections;
using UnityEngine;
using TMPro;

public class GetCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int collectibleId;

    [Header("Effects")]
    public ParticleSystem collectEffect;
    public float destroyDelay = 0.5f;
    public float shrinkSpeed = 5f;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (isCollected) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        isCollected = true;

        GameEventsManager.instance.collectibleEvents.OnCollectibleGet(collectibleId);

        StartCoroutine(PlayCollectEffect());
    }

    private IEnumerator PlayCollectEffect()
    {
        if (collectEffect != null)
        {
            collectEffect.transform.parent = null;
            collectEffect.Play();
        }

        Vector3 originalScale = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * shrinkSpeed;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);

        if (collectEffect != null)
        {
            Destroy(collectEffect.gameObject, collectEffect.main.duration);
        }
    }
}
