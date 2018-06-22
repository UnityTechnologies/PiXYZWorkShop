using UnityEngine;
using System.Collections;


public class CameraMovement : MonoBehaviour
{
    public Transform target;  //initial targeted object

    private Camera m_MainCamera; // targetcamera
    private Vector3 currentTarget;  //Current targeted point (after pan)

    private bool isMoving = false;
    public bool IsMoving { get { return isMoving; } set { isMoving = value; } }

    public float distance = 3.0f;
    private float distanceInit;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float zoomSpeed = 0.5f;
    public float panSpeed = 0.5f;

    public int xMinLimit = -360;
    public int xMaxLimit = 360;

    public int yMinLimit = -45;
    public int yMaxLimit = 85;

    public float distanceMinLimit = 0.5f;
    public float distanceMaxLimit = 10.0f;

    private float xPan = 0.0f;
    private float yPan = 0.0f;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;

    private float xRotationInit = 0.0f;
    private float yRotationInit = 0.0f;

    /// ==================================================================
    /// UNITY MONOBEHAVIOUR CALLS
    /// ================================================================== 
    void Start()
    {
        //This gets the Main Camera from the scene
        m_MainCamera = Camera.main;

        distanceInit = distance;
        currentTarget = Vector3.zero;
        if (target)
            currentTarget = target.position;

        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;

        xRotationInit = xRotation;
        yRotationInit = yRotation;
    }

    private void Update()
    {
        UpdatePos();
    }

    /// ==================================================================
    /// FUNCTIONS
    /// ================================================================== 
    public void UpdatePos()
    {
        if (target == null)
            return;

        // Zoom
        distance -= Input.mouseScrollDelta.y * zoomSpeed;
        distance = Mathf.Clamp(distance, distanceMinLimit, distanceMaxLimit);

        // Rotate
        if (Input.GetMouseButtonDown(0))
            isMoving = false;
        if (Input.GetMouseButton(0))
        {
            xRotation += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            yRotation -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            xRotation = ClampAngle(xRotation, xMinLimit, xMaxLimit);
            yRotation = ClampAngle(yRotation, yMinLimit, yMaxLimit);

            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)//detect a small change
            isMoving = true;
        }

        // Pan
        if (Input.GetMouseButton(2))
        {
            xPan = Input.GetAxis("Mouse X") * panSpeed * 0.2f;
            yPan = Input.GetAxis("Mouse Y") * panSpeed * 0.2f;
            currentTarget += this.transform.right * -xPan + this.transform.up * -yPan;
        }

        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
        Vector3 d = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * d + currentTarget;

        m_MainCamera.transform.rotation = rotation;
        m_MainCamera.transform.position = position;
    }

    public void SetTransform(Vector3 m_position, Quaternion m_rotation)
    {
        float tmp_dist;

        tmp_dist = Vector3.Distance(m_position, currentTarget);

        distance = tmp_dist;

        xRotation = m_rotation.eulerAngles.y;
        yRotation = m_rotation.eulerAngles.x;
        UpdatePos();
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    public void ResetView()
    {
        //Reset view to initial targeted object
        currentTarget = target.position;  
    
        distance = distanceInit;

        xRotation = xRotationInit;
        yRotation = yRotationInit;

        UpdatePos();
    }

    /// ==================================================================
    /// GUI EVENTS
    /// ================================================================== 
    public void OnMouseEvent()
    {
        UpdatePos();
    }
}
