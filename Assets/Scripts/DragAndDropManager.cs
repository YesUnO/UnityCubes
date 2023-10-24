using System.Linq;
using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    private bool _mouseState;
    private GameObject _target;
    private Vector3 _originalPos;
    public Vector3 screenSpace;
    public Vector3 offset;
    public bool IsDragging = false;
    private GameObject _collided;

    public static DragAndDropManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

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
                ActivateCliecked();
                IsDragging = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_target != null)
            {
                if (_collided != null)
                {
                    ItemManager.Instance.SetItemState(_collided, ItemState.Collided, false);
                    ItemManager.Instance.SwitchItemsPositions(_target, _collided);
                    _collided = null;
                }
                else
                {
                    _target.transform.position = _originalPos;
                }
            }
            IsDragging = false;
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

            GetMostOverlappedObj();
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

    private void ActivateCliecked()
    {
        var draggedItem = ItemManager.Instance.Categories.Items.SelectMany(x => x.Items).FirstOrDefault(x => x.ItemObject == _target);
        if (draggedItem != null)
        {
            Debug.Log($"item {draggedItem.Id} of {draggedItem.Category.Id}");
            ItemManager.Instance.Categories.ActivateItem(draggedItem.Category.Id);
            draggedItem.Category.ActivateItem(draggedItem.Id);
        }
    }

    private void GetMostOverlappedObj()
    {
        float maxOverlap = 0f;

        GameObject collided = null;

        Collider[] colliders = Physics.OverlapBox(_target.transform.position, _target.transform.localScale / 2f);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != _target && collider.transform.localScale == _target.transform.localScale)
            {
                Bounds bounds1 = _target.GetComponent<Collider>().bounds;
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
                }
            }
        }
        if (_collided != null && _collided != collided)
        {
            ItemManager.Instance.SetItemState(_collided, ItemState.Collided, false);
        }
        if (collided != null)
        {
            _collided = collided;
            ItemManager.Instance.SetItemState(_collided, ItemState.Collided);
        }
        else
        {
            _collided = null;
        }
    }
}
