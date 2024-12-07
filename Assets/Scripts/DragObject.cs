using UnityEngine;



public class DragObject : MonoBehaviour

{

    Vector3 mousePosition;

    private Vector3 GetMousePos()
    {
        return Camera.main.WorldToScreenPoint(transform.position);
    }


    private void OnMouseDown()
    {
        mousePosition = Input.mousePosition - GetMousePos();
    }

    private void OnMouseDrag()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePosition);
        transform.position = worldPoint;
    }

}