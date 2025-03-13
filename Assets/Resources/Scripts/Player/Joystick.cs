using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    private Vector2 inputVector = Vector2.zero;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = pos.x / background.sizeDelta.x;
            pos.y = pos.y / background.sizeDelta.y;

            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            if (inputVector.magnitude > 1.0f)
                inputVector = inputVector.normalized;

            handle.anchoredPosition = new Vector2(
                inputVector.x * (background.sizeDelta.x / 3),
                inputVector.y * (background.sizeDelta.y / 3));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    // Методы возвращают нормализованный вектор, чтобы скорость движения оставалась постоянной
    public float Horizontal()
    {
        return inputVector != Vector2.zero ? inputVector.normalized.x : 0f;
    }

    public float Vertical()
    {
        return inputVector != Vector2.zero ? inputVector.normalized.y : 0f;
    }
}