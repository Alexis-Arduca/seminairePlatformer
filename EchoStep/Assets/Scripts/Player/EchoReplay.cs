using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EchoReplay : MonoBehaviour
{
    private CharacterController controller;

    private List<EchoFrameData> recordedFrames = new List<EchoFrameData>();
    private float replayDuration;
    private float timer;

    [Header("Echo Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 2f;

    [Header("Text Frames")]
    public List<Texture> frames;
    private TMP_Text textReplayTimer;

    private Vector3 velocity;
    private float gravity = -9.81f;

    void Awake()
    {
        GameObject textObj = GameObject.FindGameObjectWithTag("EchoText");
        if (textObj == null)
            textObj = GameObject.Find("EchoText");

        if (textObj != null)
        {
            textReplayTimer = textObj.GetComponent<TMP_Text>();
            textReplayTimer.gameObject.SetActive(false);
        }
    }

    public void StartReplay(List<EchoFrameData> frames, float duration)
    {
        recordedFrames = frames;
        replayDuration = duration;
        controller = GetComponent<CharacterController>();
        timer = 0f;

        if (textReplayTimer != null)
        {
            textReplayTimer.gameObject.SetActive(true);
            textReplayTimer.text = $"Echo Replay: {replayDuration:0.0}s";
        }

        StartCoroutine(ReplayRoutine());
    }

    private IEnumerator ReplayRoutine()
    {
        for (int i = 0; i < recordedFrames.Count; i++)
        {
            timer += Time.deltaTime;

            if (textReplayTimer != null)
            {
                float remaining = Mathf.Max(0, replayDuration - timer);
                textReplayTimer.text = $"Echo Replay: {remaining:0.0}s";
            }

            EchoFrameData frame = recordedFrames[i];
            transform.rotation = Quaternion.Euler(0f, frame.rotationY, 0f);

            Vector3 move = Vector3.zero;
            foreach (string input in frame.inputs)
            {
                if (input == "W") move += transform.forward;
                if (input == "S") move -= transform.forward;
                if (input == "A") move -= transform.right;
                if (input == "D") move += transform.right;
                if (input == "Jump" && controller.isGrounded)
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            controller.Move(move * moveSpeed * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            yield return null;
        }

        velocity = Vector3.zero;
        controller.Move(Vector3.zero);

        if (textReplayTimer != null)
            StartCoroutine(HideTextAfterDelay(0.5f));

        yield return new WaitForSeconds(0.1f);

        GameEventsManager.instance.playerEvents.OnPlayerActiveEcho();
        Destroy(gameObject, 1f);
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        textReplayTimer.text = $"No record available";
        GameEventsManager.instance.playerEvents.OnPlayerActiveRecord(frames);
    }
}
