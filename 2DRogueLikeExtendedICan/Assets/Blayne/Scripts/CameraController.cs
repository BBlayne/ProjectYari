using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to player gameObject.
/// Assumes only one camera.
/// If the player is too far from camera,
/// have the camera lerp back to player.
/// </summary>
public class CameraController : MonoBehaviour {
    bool isMoving = false;
	// Use this for initialization
	void Start () {
        ResizeCamera(10);
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 oldCameraPosition = Camera.main.gameObject.transform.position;
        oldCameraPosition.z = 0;
        oldCameraPosition.y = 0;
        Vector3 oldPlayerPosition = this.gameObject.transform.position;
        oldPlayerPosition.z = 0;
        oldPlayerPosition.y = 0;

        float HoriztonalDistance = Vector3.Distance(oldPlayerPosition,
            oldCameraPosition);


        oldCameraPosition = Camera.main.gameObject.transform.position;
        oldCameraPosition.x = 0;
        oldCameraPosition.z = 0;
        oldPlayerPosition = this.gameObject.transform.position;
        oldPlayerPosition.x = 0;
        oldPlayerPosition.z = 0;


        float VerticalDistance = Vector3.Distance(oldPlayerPosition,
            oldCameraPosition);

        if (VerticalDistance > 4 || HoriztonalDistance > 8)
        {
            isMoving = true;
            UpdateCamera();
        }
        else
        {
            if (isMoving)
            {
                
                oldCameraPosition = Camera.main.gameObject.transform.position;
                oldPlayerPosition = this.gameObject.transform.position;
                oldCameraPosition.z = 0;
                oldPlayerPosition.z = 0;
                float distance = Vector3.Distance(oldPlayerPosition,
                     oldCameraPosition);
                if (distance < 0.125f)
                {
                    isMoving = false;
                }
                UpdateCamera();
            }
            else
            {
                // Make sure the position is clean.
                Vector3 newCameraPos = Camera.main.gameObject.transform.position;
                newCameraPos.x = Mathf.Round(newCameraPos.x);
                newCameraPos.y = Mathf.Round(newCameraPos.y);
                newCameraPos.z = -10;
                Camera.main.gameObject.transform.position = newCameraPos;
            }
        }
        
	}

    void UpdateCamera()
    {
        Vector3 oldCameraPosition = Camera.main.gameObject.transform.position;
        oldCameraPosition.z = 0;
        Vector3 oldPlayerPosition = this.gameObject.transform.position;
        oldPlayerPosition.z = 0;

        oldCameraPosition = Vector3.Slerp(oldCameraPosition,
            oldPlayerPosition,
            Time.deltaTime * 3);
        
        oldCameraPosition.z = -10;
        Camera.main.gameObject.transform.position = oldCameraPosition;
    }

    void ResizeCamera(int _size)
    {
        Camera.main.orthographicSize = _size / 2;
    }
}
