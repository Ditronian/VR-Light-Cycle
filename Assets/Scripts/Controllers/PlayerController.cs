using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerController : MonoBehaviour
{

    public GameObject model;
    public GameObject leftHand;
    public GameObject rightHand;
    
    private XRController leftHandController;
    private XRController rightHandController;
    public InputDevice leftHandInput;
    public InputDevice rightHandInput;
    public GameObject leftHandle;
    public GameObject rightHandle;
    private HandleController leftHandleController;
    private HandleController rightHandleController;
    public RenderTexture minimap;

    private bool leftGripPressed;
    private bool leftTriggerPressed;
    private bool rightGripPressed;
    private bool rightTriggerPressed;
    private bool handOnBars;
    public float barAngle;
    public float maximumRoll;
    public float sensitivity;


    void Awake()
    {
        leftHandController = leftHand.GetComponent<XRController>();
        rightHandController = rightHand.GetComponent<XRController>();
        leftHandInput = leftHandController.inputDevice;
        rightHandInput = rightHandController.inputDevice;
        leftHandleController = leftHandle.GetComponent<HandleController>();
        rightHandleController = rightHandle.GetComponent<HandleController>();
        minimap = transform.GetChild(4).GetComponent<RenderTexture>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Send turnValue to server
        //ClientSend.PlayerMovement(turnValue);

        //if(handOnBars) SendInputToServer();
        CheckHandOnBars();
        SendInputToServer();
    }

    //Checks if player's hands are on the bars and if they are gripping.  Releases grip when the player does.
    private void CheckHandOnBars()
    {
        //Check if hands on bars
        if (!handOnBars && leftHandleController.handOnBars && rightHandleController.handOnBars)
        {
            leftHandInput.TryGetFeatureValue(CommonUsages.gripButton, out leftGripPressed);
            leftHandInput.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerPressed);
            rightHandInput.TryGetFeatureValue(CommonUsages.gripButton, out rightGripPressed);
            rightHandInput.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerPressed);

            //Check if gripping bars, and if so set to true
            if ((leftGripPressed || leftTriggerPressed) && (rightGripPressed || rightTriggerPressed)) handOnBars = true;
        }
        //Release grip on bars
        //else if(handOnBars && !(leftGripPressed || leftTriggerPressed) && !(rightGripPressed || rightTriggerPressed)) handOnBars = false;

        //Check if gripping bars, and if so math out the angle of the bars
        if (handOnBars)
        {
            float x;
            float y;
            float distance = Vector3.Distance(leftHand.transform.position, rightHand.transform.position);

            x = Mathf.Abs(rightHand.transform.position.x - leftHand.transform.position.x);
            y = leftHand.transform.position.y - rightHand.transform.position.y;
            if (y < 0) barAngle = -(Mathf.Asin(Mathf.Abs(y) / distance) * Mathf.Rad2Deg * sensitivity);
            else if (y > 0) barAngle = (Mathf.Asin(Mathf.Abs(y) / distance) * Mathf.Rad2Deg * sensitivity);

            if (barAngle < -maximumRoll) barAngle = -maximumRoll;
            else if (barAngle > maximumRoll) barAngle = maximumRoll;
        }

        Vector3 current = transform.eulerAngles;
        transform.eulerAngles = new Vector3(current.x, current.y, -barAngle);
    }

    //Handles the rolling of the bike if right conditions are met.
    private void SendInputToServer()
    {
        ClientSend.PlayerTurn(barAngle);
    }
}
