using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIcamera : MonoBehaviour
{
    [SerializeField] int sensHorizontal;
    [SerializeField] int SensVertical;

    [SerializeField] int lockVerticalMin;
    [SerializeField] int lockVerticalMax;

    [SerializeField] bool inverse;
    [SerializeField] bool mouseLook;

    float xRotation;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ToggleMouseLook();
        MouseLook();

    }
    private void ToggleMouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            mouseLook = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            mouseLook = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
    private void MouseLook()
    {
        if (mouseLook)
        {
            //Get Input
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensHorizontal;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * SensVertical;

            if (inverse)
                xRotation += mouseY;
            else
                xRotation -= mouseY;

            //Clamp Rotation
            xRotation = Mathf.Clamp(xRotation, lockVerticalMin, lockVerticalMax);

            //Rotate the Camera on the X-Axis
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            //Rotate the Player on the Y-Axis
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }
}
