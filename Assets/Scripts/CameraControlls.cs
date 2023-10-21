using UnityEngine;

public class CameraControlls : MonoBehaviour
{
    public Vector3 targetPosition;
    public float rotationSpeed = 60.0f;
    private float _distance = 10f;
    public float minDistance = 5.0f;
    public float maxDistance = 20.0f;
    public float scrollSpeed = 2.0f;

    void Start()
    {
    }

    void Update()
    {

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var step = rotationSpeed * Time.deltaTime;

        var normalized = new Vector3(horizontal * step, vertical * step, 0).normalized;
        transform.Translate(new Vector3(horizontal * step, vertical * step, 0));

        transform.LookAt(targetPosition);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _distance -= scroll * scrollSpeed;

        _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

        transform.position = transform.position.normalized * _distance;
    }
}
