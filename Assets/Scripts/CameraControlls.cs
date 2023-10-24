using UnityEngine;

public class CameraControlls : MonoBehaviour
{
    public Vector3 targetPosition;
    public Vector3 _newTargetPosition;
    public float rotationSpeed = 60.0f;
    private float _distance = 10f;
    public float minDistance = 5.0f;
    public float maxDistance = 20.0f;
    public float scrollSpeed = 15.0f;

    private float _cubeDistance;
    private Vector3 _centroid;

    private bool _targetMoving = false;

    void Start()
    {
    }

    void Update()
    {
        if (_targetMoving)
        {
            EaseIntoNewPosition();
        }

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var step = rotationSpeed * Time.deltaTime;

        transform.Translate(new Vector3(horizontal * step, vertical * step, 0));

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _distance -= scroll * scrollSpeed;
        _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

        Vector3 direction = transform.position - targetPosition;
        direction = direction.normalized * _distance;
        transform.position = targetPosition + direction;
        transform.LookAt(targetPosition);
    }

    public void AdjustTargetPosition(Vector3 centroid, float cubeDistance)
    {
        if (centroid != _centroid || cubeDistance != _cubeDistance)
        {
            _centroid = centroid;
            _cubeDistance = cubeDistance;
            maxDistance = Mathf.Max(20f, _centroid.magnitude * 1.5f * _cubeDistance);
            _newTargetPosition = _centroid * _cubeDistance * 1.5f;
            _targetMoving = true;
        }
    }

    private void EaseIntoNewPosition()
    {
        targetPosition = Vector3.MoveTowards(targetPosition, _newTargetPosition, Time.deltaTime * 5f);
        if (targetPosition == _newTargetPosition)
        {
            _targetMoving = false;
        }
    }
}
