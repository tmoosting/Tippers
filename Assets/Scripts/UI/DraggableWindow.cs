using UnityEngine;
using UnityEngine.UIElements;

public class DraggableWindow : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement window;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        window = uiDocument.rootVisualElement.Q("WindowBase"); 

        window.RegisterCallback<MouseDownEvent>(OnMouseDown);
        window.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        window.RegisterCallback<MouseUpEvent>(OnMouseUp);

        isDragging = false;
    }

    private bool isDragging;
    private Vector2 startMousePosition;
    private Vector2 startPosition;

    void OnMouseDown(MouseDownEvent evt)
    {
        isDragging = true;
        startMousePosition = evt.mousePosition;
        startPosition = window.transform.position;
    }

    void OnMouseMove(MouseMoveEvent evt)
    {
        if (isDragging)
        {
            Vector2 delta = evt.mousePosition - startMousePosition;
            window.transform.position = startPosition + delta;
        }
    }

    void OnMouseUp(MouseUpEvent evt)
    {
        isDragging = false;
    }
}