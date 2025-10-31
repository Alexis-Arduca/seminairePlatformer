using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

[System.Serializable]
public class EchoFrameData
{
    public List<string> inputs = new List<string>();
    public float rotationY;
    public float rotationX;
}

public class EchoMechanics : MonoBehaviour
{
    [Header("Echo Prefab")]
    public GameObject echoPrefab;
    public TextFade textFade;

    [Header("UI Elements")]
    public List<Texture> frames;
    public TMP_Text textRecording;

    private Vector3 echoStartPosition;
    private float recordDuration;
    private float timer;
    private bool isRecording;

    private List<EchoFrameData> recordedFrames = new List<EchoFrameData>();

    private void Update()
    {
        if (isRecording)
        {
            RecordInputs();
            UpdateRecordingTimer();
        }
    }

    /// <summary>
    /// Record Player input
    /// </summary>
    public void CallEchoRecorder(Vector3 startPosition, float echoRecordTime)
    {
        recordedFrames.Clear();

        echoStartPosition = startPosition;
        recordDuration = echoRecordTime;
        timer = 0f;
        isRecording = true;

        if (textRecording != null)
        {
            textRecording.gameObject.SetActive(true);
            textRecording.text = $"Echo Recording: {recordDuration:0.0}s";
        }
    }

    private void RecordInputs()
    {
        timer += Time.deltaTime;

        EchoFrameData frame = new EchoFrameData();

        if (Input.GetKey(KeyCode.W)) frame.inputs.Add("W");
        if (Input.GetKey(KeyCode.A)) frame.inputs.Add("A");
        if (Input.GetKey(KeyCode.S)) frame.inputs.Add("S");
        if (Input.GetKey(KeyCode.D)) frame.inputs.Add("D");
        if (Input.GetKey(KeyCode.Space)) frame.inputs.Add("Jump");

        frame.rotationY = transform.rotation.eulerAngles.y;

        recordedFrames.Add(frame);

        if (timer >= recordDuration)
        {
            isRecording = false;

            if (textRecording != null)
                StartCoroutine(HideRecordingText());

            GameEventsManager.instance.playerEvents.OnPlayerActiveEcho();
        }
    }

    private void UpdateRecordingTimer()
    {
        if (textRecording != null)
        {
            float remaining = Mathf.Max(0, recordDuration - timer);
            textRecording.text = $"Recording: {remaining:0.0}s";
        }
    }

    private IEnumerator HideRecordingText()
    {
        yield return new WaitForSeconds(0.5f);
        textRecording.text = "Echo Recorded";
        GameEventsManager.instance.playerEvents.OnPlayerActiveRecord(frames);
    }

    /// <summary>
    /// Instantiate the Echo Prefab and play its replay
    /// </summary>
    public void CallEchoActivation()
    {
        if (recordedFrames.Count == 0)
        {
            textFade.PlayFade("No Echo Record available");
            return;
        }

        GameObject echo = Instantiate(echoPrefab, echoStartPosition, Quaternion.identity);

        EchoAppearance appearance = echo.GetComponent<EchoAppearance>();
        EchoReplay replay = echo.GetComponent<EchoReplay>();

        if (replay != null)
        {
            float delay = appearance != null ? appearance.AppearDuration : 0f;
            StartCoroutine(StartReplayAfterDelay(replay, recordedFrames, recordDuration, delay));
        }
    }

    private IEnumerator StartReplayAfterDelay(EchoReplay replay, List<EchoFrameData> frames, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        replay.StartReplay(frames, duration);
    }
}
