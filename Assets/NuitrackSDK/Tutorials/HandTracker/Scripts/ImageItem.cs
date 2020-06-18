﻿using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Collections.Generic;

public class ImageItem : Selectable, IDragHandler
{
    public delegate void Click(ImageItem currentItem);
    public event Click OnClick;

    List<PointerEventData> touches = new List<PointerEventData>();

    Vector3 startCenter;
    Vector3 startPosition;

    Vector3 startScale;
    float startHandDistance;

    float startAngle;
    Quaternion startRotation;

    public override void OnPointerExit(PointerEventData eventData)
    {
        touches.Remove(eventData);
        base.OnPointerExit(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!touches.Contains(eventData))
        {
            touches.Add(eventData);
            UpdateInitialState();
        }

        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        touches.Remove(eventData);
        UpdateInitialState();

        OnClick(this);
        InstantClearState();

        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (interactable || !eventData.dragging)
            return;

        if(OneTouch)
        {
            Vector3 currentCenter = touches[0].position;
            Rect.anchoredPosition = startPosition + (currentCenter - startCenter);
        }
        else if(MultiTouch)
        {
            Vector3 currentCenter = (touches[0].position + touches[1].position) / 2;
            Rect.anchoredPosition = startPosition + (currentCenter - startCenter);

            float currentHandDistance = (touches[0].position - touches[1].position).magnitude;
            Rect.localScale = startScale * Mathf.Abs(currentHandDistance / startHandDistance);

            Vector3 pointRelativeToZero = touches[1].position - touches[0].position;
            float angle = Mathf.Atan2(pointRelativeToZero.x, pointRelativeToZero.y) * Mathf.Rad2Deg;

            Rect.localRotation = startRotation * Quaternion.Euler(0, 0, startAngle - angle);
        }
    }

    void UpdateInitialState()
    {
        if (OneTouch)
        {
            startCenter = touches[0].position;  
        }
        else if (MultiTouch)
        {
            startCenter = (touches[0].position + touches[1].position) / 2;
            startHandDistance = (touches[0].position - touches[1].position).magnitude;

            Vector3 pointRelativeToZero = touches[1].position - touches[0].position;
            startAngle = Mathf.Atan2(pointRelativeToZero.x, pointRelativeToZero.y) * Mathf.Rad2Deg;
        }

        startScale = Rect.localScale;
        startPosition = Rect.anchoredPosition;
        startRotation = Rect.localRotation;
    }

    bool MultiTouch
    {
        get
        {
            return touches.Count > 1;
        }
    }

    bool OneTouch
    {
        get
        {
            return touches.Count == 1;
        }
    }

    public RectTransform Rect
    {
        get
        {
            return image.rectTransform;
        }
    }
}
