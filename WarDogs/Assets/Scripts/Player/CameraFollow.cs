using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //public bool FollowTargetRotation;
    [Header("FollowSpeed")]
    public float followSpeed = 0.5f;
    public float followSpeedFlying = 10f;
    public float gravityFollowSpeed = 0.1f;
    private Vector3 lookDirection;

    public Transform target;
    public Transform followTarget;
    public Transform yAxisPiyot;

    private Transform pivot;
    private Transform followRotationPivot;
    public Transform camTransform;
    private Camera cam;

    private Vector3 lookAtPos;
    [Header("Mouse Speeds")]
    public float mouseSpeed = 2;
    public float turnSmoothing = 0.1f;
    public float minAngle = -35;
    public float maxAngle = 35;
    public float lookDirectionSpeed = 2f;

    public float distanceFromPlayer;
    private float currentDistance;

    float smoothX;
    float smoothXVelocity;
    float smoothY;
    float smoothYVelocity;
    private float lookAngle;
    private float tiltAngle;

    float delta;

    //setup objects
    void Awake()
    {
        transform.parent = null;

        pivot = camTransform.parent;
        lookAtPos = target.position;
        currentDistance = distanceFromPlayer;

        tiltAngle = 10f;

        lookDirection = transform.forward;

        cam = GetComponentInChildren<Camera>();
    }
    private void Update()
    {
        transform.position = followTarget.position;
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;

        if (!target)
        {
            return;
        }
        Tick(delta);
    }

    public void Tick(float d)
    {
        float h = Input.GetAxis("CamHorizontal");
        float v = Input.GetAxis("CamVertical");
        float rotateSpeed = mouseSpeed;

        HandleRotation(d, v, h, rotateSpeed);
        handlePivotPosition();

        //look at player
        
        
        lookAtPos = target.position;

        Vector3 LerpDir = Vector3.Lerp(transform.up, target.up, d * followSpeed);
        transform.rotation = Quaternion.FromToRotation(transform.up, LerpDir) * transform.rotation;
    }

    void handlePivotPosition()
    {
        float targetZ = distanceFromPlayer;

        currentDistance = Mathf.Lerp(currentDistance, targetZ, delta * 5f);

        Vector3 tp = Vector3.zero;
        tp.z = currentDistance;
        camTransform.localPosition = tp;
    }

    void HandleRotation(float d, float v, float h, float speed)
    {
        if (turnSmoothing > 0)
        {
            smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYVelocity, turnSmoothing);
            smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXVelocity, turnSmoothing);
        }
        else
        {
            smoothX = h;
            smoothY = v;
        }

        tiltAngle -= smoothY * speed;
        tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
        pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

        lookAngle += smoothX * speed;
        if (lookAngle > 360)
            lookAngle = 0;
        else if (lookAngle < 0)
            lookAngle = 360;

        if (smoothX != 0)
        {
            transform.RotateAround(transform.position, transform.up, ((smoothX * speed) * 30f) * d);
        }
    }
}