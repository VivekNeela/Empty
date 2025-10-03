using TMKOC.Reusable;
using UnityEngine;

public class TestDraggable : Draggable
{
    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
    }

    protected override void OnSelected()
    {
        Debug.Log("Object selected: " + gameObject.name);
    }

    protected override void OnDeselected()
    {
        transform.position = initialPos;
        Debug.Log("Object deselected: " + gameObject.name + ", returning to initial position.");
    }

}
