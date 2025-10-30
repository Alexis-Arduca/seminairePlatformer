using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EchoReplay : MonoBehaviour
{
    private CharacterController controller;
    private List<EchoFrameData> recordedFrames = new List<EchoFrameData>();
    private float replayDuration;
    private float timer;
    private int currentIndex;

    public float moveSpeed = 5f;
    public float jumpForce = 2f;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private bool isGrounded;

    /// <summary>
    /// Init the echo inputs
    /// </summary>
    /// <param name="frames">List of inputs</param>
    /// <param name="duration">Duration of the replay</param>
    public void StartReplay(List<EchoFrameData> frames, float duration)
    {
        recordedFrames = frames;
        replayDuration = duration;
        controller = GetComponent<CharacterController>();
        StartCoroutine(ReplayRoutine());
    }

    /// <summary>
    /// Remplay each input made from the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReplayRoutine()
    {
        for (int i = 0; i < recordedFrames.Count; i++)
        {
            EchoFrameData frame = recordedFrames[i];

            transform.rotation = Quaternion.Euler(0f, frame.rotationY, 0f);

            Vector3 move = Vector3.zero;
            foreach (string input in frame.inputs)
            {
                if (input == "W") move += transform.forward;
                if (input == "S") move -= transform.forward;
                if (input == "A") move -= transform.right;
                if (input == "D") move += transform.right;
                if (input == "Jump" && controller.isGrounded) { velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity); }
            }

            controller.Move(move * moveSpeed * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            yield return null;
        }

        Destroy(gameObject, 1f);
    }
}
