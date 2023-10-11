using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elegy.UI
{
    

    public class CameraControl : MonoBehaviour
    {
        private float orthographicSize;
        private float aspectRatio;
        private float camHeight;
        private float camWidth;

        [Header("Borders")]
        [SerializeField] private GameObject bottomLeftBorder;
        [SerializeField] private GameObject topRightBorder;

        [Header("Details")]
        [SerializeField] private float cameraSpeed;
        [SerializeField] private float smoothTime;

        [Header("Keyboard Controls")]
        [SerializeField] private Vector3 keyboardInput;

        [Header("Mouse Control")]
        [SerializeField] private float padding;
        [SerializeField] private Vector3 cursorPosition;
        [SerializeField] private Vector3 targetCameraPosition;

        private void Start()
        {
            orthographicSize = Camera.main.orthographicSize;
            aspectRatio = Camera.main.aspect;
            camHeight = orthographicSize * 2;
            camWidth = camHeight * aspectRatio;
        }

        private void Update()
        {
            MoveCameraKey();
            MoveCameraMouse();
        }

        private Vector3 ConfineInBounds(Vector3 position)
        {
            float halfWidth = camWidth / 2;
            float halfHeight = camHeight / 2;
            position.x = Mathf.Clamp(position.x, bottomLeftBorder.transform.position.x + halfWidth, topRightBorder.transform.position.x - halfWidth);
            position.z = Mathf.Clamp(position.z, bottomLeftBorder.transform.position.z + halfHeight, topRightBorder.transform.position.z - halfHeight);
            return position;
        }

        private void MoveCameraKey()
        {
            Vector3 position = transform.position;
            position += (keyboardInput * cameraSpeed * Time.deltaTime);
            position = ConfineInBounds(position);
            transform.position = position;
        }

        private void MoveCameraMouse()
        {
            // Read the cursor position
            cursorPosition = Mouse.current.position.ReadValue();

            // Get the current camera position.
            Vector3 cameraPos = transform.position;
            targetCameraPosition = cameraPos;

            if (cursorPosition.x < padding)
            {
                targetCameraPosition.x = Mathf.Lerp(cameraPos.x, cameraPos.x - cameraSpeed * Time.deltaTime, Mathf.SmoothStep(0, 1, 1 - cursorPosition.x / padding));
            }
            else if (cursorPosition.x > Screen.width - padding)
            {
                targetCameraPosition.x = Mathf.Lerp(cameraPos.x, cameraPos.x + cameraSpeed * Time.deltaTime, Mathf.SmoothStep(0, 1, (cursorPosition.x - (Screen.width - padding)) / padding));
            }

            if (cursorPosition.y < padding)
            {
                targetCameraPosition.z = Mathf.Lerp(cameraPos.z, cameraPos.z - cameraSpeed * Time.deltaTime, Mathf.SmoothStep(0, 1, 1 - cursorPosition.y / padding));
            }
            else if (cursorPosition.y > Screen.height - padding)
            {
                targetCameraPosition.z = Mathf.Lerp(cameraPos.z, cameraPos.z + cameraSpeed * Time.deltaTime, Mathf.SmoothStep(0, 1, (cursorPosition.y - (Screen.height - padding)) / padding));
            }

            // Confine the target position
            targetCameraPosition = ConfineInBounds(targetCameraPosition);

            // Lerp to the position
            transform.position = Vector3.Lerp(transform.position, targetCameraPosition, smoothTime);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            // Assign the movement vector
            if(context.started || context.performed)
            {
                keyboardInput = context.ReadValue<Vector3>();
            }
        }
    }
}
