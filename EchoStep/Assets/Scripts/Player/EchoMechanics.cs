using UnityEngine;
using System.Collections.Generic;

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

    private Vector3 echoStartPosition;
    private float recordDuration;
    private float timer;
    private bool isRecording;
    private bool startEcho = false;

    private List<EchoFrameData> recordedFrames = new List<EchoFrameData>();

    private void Update()
    {
        if (isRecording)
        {
            RecordInputs();
        }
    }

    /// <summary>
    /// Record Player input
    /// </summary>
    /// <param name="startPosition">Position where the player start the record, use to instantiate the Echo at this position</param>
    /// <param name="echoRecordTime">Record Time</param>
    public void CallEchoRecorder(Vector3 startPosition, float echoRecordTime)
    {
        recordedFrames.Clear();

        echoStartPosition = startPosition;
        recordDuration = echoRecordTime;
        timer = 0f;
        isRecording = true;
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

        if (timer >= recordDuration) { isRecording = false; GameEventsManager.instance.playerEvents.OnPlayerActiveEcho(); }
    }

    /// <summary>
    /// Instantiate the Echo Prefab and call is function to apply Input
    /// </summary>
    public void CallEchoActivation()
    {
        if (recordedFrames.Count == 0) return;

        GameObject echo = Instantiate(echoPrefab, echoStartPosition, Quaternion.identity);
        EchoReplay replay = echo.GetComponent<EchoReplay>();

        if (replay != null) { replay.StartReplay(recordedFrames, recordDuration); }
    }
}
