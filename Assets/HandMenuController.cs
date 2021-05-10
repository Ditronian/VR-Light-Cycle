using System.Collections;
using System.Collections.Generic;
using TalesFromTheRift;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandMenuController : MonoBehaviour
{
    [SerializeField] LineRenderer rend;
    Vector3[] points;
    public LayerMask layerMask;

    public GameObject panel;
    public Button button;
    public InputField usernameField;
    public InputField ipField;
    public InputField portField;

    private CanvasKeyboard keyboard = null;
    public Canvas CanvasKeyboardObject;
    public GameObject inputObject; // Optional: Input Object to receive text

    private XRController leftHandController;
    private InputDevice leftHandInput;
    private bool leftTriggerPressed;
    private bool firstOccurance = true;

    void Start()
    {
        leftHandController = GetComponent<XRController>();
        rend = gameObject.GetComponent<LineRenderer>();
        leftHandInput = leftHandController.inputDevice;


        points = new Vector3[2];
        points[0] = Vector3.zero;
        points[1] = transform.position + new Vector3(0,0, 20f);

        rend.SetPositions(points);
        rend.enabled = true;
    }

    public GameObject AlignLineRenderer(LineRenderer rend)
    {
        Ray ray;
        ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        GameObject collider = null;

        if (Physics.Raycast(ray, out hit))
        {
            points[1] = transform.forward + new Vector3(0, 0, hit.distance);
            rend.startColor = Color.green;
            rend.endColor = Color.green;

            collider = hit.collider.gameObject;
        }
        else
        {
            points[1] = transform.forward + new Vector3(0, 0, 20);
            rend.startColor = Color.red;
            rend.endColor = Color.red;
        }

        rend.SetPositions(points);
        rend.material.color = rend.startColor;
        return collider;
    }

    void Update()
    {
        GameObject collider = AlignLineRenderer(rend);
        leftHandInput.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerPressed);

        if (collider != null && leftTriggerPressed && firstOccurance)
        {
            firstOccurance = false;
            if (collider == button.gameObject) button.onClick.Invoke();
            else if (collider == usernameField.gameObject)
            {
                usernameField.Select();
                ToggleKeyboard(usernameField.gameObject);
            }
            else if (collider == ipField.gameObject)
            {
                ipField.Select();
                ToggleKeyboard(ipField.gameObject);
            }
            else if (collider == portField.gameObject)
            {
                portField.Select();
                ToggleKeyboard(portField.gameObject);
            }
            else if (collider.tag == "kb")
            {
                Button kbButton = collider.GetComponent<Button>();
                kbButton.onClick.Invoke();
            }
        }
        else if (!firstOccurance && !leftTriggerPressed) firstOccurance = true;
    }

    public void ToggleKeyboard(GameObject focus)
    {
        if (keyboard == null) keyboard = CanvasKeyboard.Open(CanvasKeyboardObject);
        keyboard.inputObject = focus;
    }

}
