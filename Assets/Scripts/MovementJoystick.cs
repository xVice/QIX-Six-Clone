using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform joystick;
    public RectTransform joystickBG;
    public Vector2 joystickVec;
    private Vector2 joystickOriginalPos;
    private float joystickRadius;

    [SerializeField]
    private float sensitivity = 1.0f;

    [SerializeField]
    private float joystickMaxDistance = 50.0f;

    // Start is called before the first frame update
    void Start()
    {
        joystickOriginalPos = joystickBG.position;
        joystickRadius = joystickBG.sizeDelta.y / 4;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBG, eventData.position, eventData.pressEventCamera, out dragPos))
        {
            dragPos = Vector2.ClampMagnitude(dragPos, joystickMaxDistance);
            joystickVec = dragPos.normalized * sensitivity;

            joystick.anchoredPosition = dragPos;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickVec = Vector2.zero;
        joystick.anchoredPosition = Vector2.zero;
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
    }

    public void SetJoystickMaxDistance(float value)
    {
        joystickMaxDistance = value;
    }
}
