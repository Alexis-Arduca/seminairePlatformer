using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Parameters")]
    public float echoRecordTime = 3f;
    public bool isButtonPressed = false;
    

    [Header("Player Components")]
    public EchoMechanics echoMechanics;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) { echoMechanics.CallEchoRecorder(this.gameObject.transform.position, echoRecordTime); }
        if (Input.GetKeyDown(KeyCode.R)) { echoMechanics.CallEchoActivation(); }
    }
}
