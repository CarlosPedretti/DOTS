using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Playershoot : MonoBehaviour
{
    [SerializeField] Transform bulletPrefab;
    public int shotPower = 2;
    private int maxShotPower = 100;

    public Text shotPowerText;

    private float mouseSensitivity = 100.0f;
    private float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    public float movespeed = 2f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        Cursor.lockState = CursorLockMode.Locked;

        shotPower = Mathf.Clamp(shotPower, 1, maxShotPower);
        shotPowerText.text = shotPower.ToString() + " (Scroll to change)";
    }

    void Update()
    {
        // Position
        transform.position += ((transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"))) * Time.deltaTime * movespeed;

        // Rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotX += mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;

        // Shooting
        if (Input.GetMouseButtonDown(0))
        {

            Cursor.lockState = CursorLockMode.Locked;
            Bullet newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
            newBullet.Shoot(shotPower * 2);
        }

        // lock Cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }

        // shot Power
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");

        if(scrollValue != 0)
        {
            if (scrollValue > 0)
                scrollValue = 1;
            else
                scrollValue = -1;

            shotPower += (int)scrollValue;
            shotPower = Mathf.Clamp(shotPower, 1, maxShotPower);
            shotPowerText.text = shotPower.ToString() + " (Scroll to change)";
        }
    }
}
