using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    private bool _mouseState;
    private GameObject _target;
    private Vector3 _originalPos;
    public Vector3 screenSpace;
    public Vector3 offset;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit hitInfo;
            _target = GetClickedObject(out hitInfo);
            if (_target != null)
            {
                _mouseState = true;
                screenSpace = Camera.main.WorldToScreenPoint(_target.transform.position);
                offset = _target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                _originalPos = _target.transform.position;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_target != null)
            {
                if (TryGetMostOverlappedObj(_target,out var collided))
                {
                    ItemManager.Instance.SwitchItemsPositions(_target, collided);
                }
                else
                {
                    _target.transform.position = _originalPos;
                }
            }
            _target = null;
            _mouseState = false;
        }
        if (_mouseState)
        {
            //keep track of the mouse position
            var curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);

            //convert the screen mouse position to world point and adjust with offset
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;

            //update the position of the object in the world
            _target.transform.position = curPosition;
        }
    }


    GameObject GetClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            target = hit.collider.gameObject;
        }

        return target;
    }

    bool TryGetMostOverlappedObj(GameObject cube, out GameObject collided)
    {
        float maxOverlap = 0f;
        collided = null;

        Collider[] colliders = Physics.OverlapBox(cube.transform.position, cube.transform.localScale / 2f);
        
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != cube && collider.transform.localScale == cube.transform.localScale)
            {
                Bounds bounds1 = cube.GetComponent<Collider>().bounds;
                Bounds bounds2 = collider.bounds;
                float overlapX = Mathf.Min(bounds1.max.x, bounds2.max.x) - Mathf.Max(bounds1.min.x, bounds2.min.x);
                float overlapY = Mathf.Min(bounds1.max.y, bounds2.max.y) - Mathf.Max(bounds1.min.y, bounds2.min.y);
                float overlapZ = Mathf.Min(bounds1.max.z, bounds2.max.z) - Mathf.Max(bounds1.min.z, bounds2.min.z);

                float volume1 = bounds1.size.x * bounds1.size.y * bounds1.size.z;
                float volume2 = bounds2.size.x * bounds2.size.y * bounds2.size.z;
                float overlapVolume = Mathf.Max(overlapX, 0) * Mathf.Max(overlapY, 0) * Mathf.Max(overlapZ, 0);

                // Calculate the ratio of overlap to the volume of the dropped cube
                float overlapRatio = overlapVolume / volume1;

                if (overlapRatio >= maxOverlap)
                {
                    maxOverlap = overlapRatio;
                    collided = collider.gameObject;
                    return true; 
                }
            }
        }

        return false; // No significant overlap detected
    }
}
