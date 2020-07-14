using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HandsInputModule: PointerInputModule
{
    GameObject currentLookAtHandler;

    List<RaycastResult> raycastResults = new List<RaycastResult>();

    [SerializeField]
    List<Pointer> pointers;

    Dictionary<Pointer, PointerEventData> pointerEvents = new Dictionary<Pointer, PointerEventData>();

    protected override void Awake()
    {
        base.Awake();

        foreach (Pointer p in pointers)
            pointerEvents.Add(p, new PointerEventData(eventSystem));
    }

    public override void Process()
    {
        foreach (KeyValuePair<Pointer, PointerEventData> pe in pointerEvents)
        {
            Pointer p = pe.Key;
            PointerEventData pointerEventData = pe.Value;

            Vector2 pointOnScreenPosition = Camera.main.WorldToScreenPoint(p.transform.position);
            pointerEventData.delta = pointOnScreenPosition - pointerEventData.position;
            pointerEventData.position = pointOnScreenPosition;

            raycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, raycastResults);
            pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

            if (p.Press)
            {
                if (pointerEventData.pointerDrag == null)
                {
                    pointerEventData.pressPosition = pointOnScreenPosition;
                    pointerEventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(pointerEventData.pointerCurrentRaycast.gameObject);
                }
            }
            else
                pointerEventData.pointerDrag = null;

            ProcessMove(pointerEventData);
            ProcessDrag(pointerEventData);

            if (pointerEventData.pointerEnter != null)
            {
                GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEventData.pointerEnter);

                if (handler != null && p.Press)
                    ExecuteEvents.ExecuteHierarchy(handler, pointerEventData, ExecuteEvents.pointerClickHandler);
            }
        }
    }

    //protected override void ProcessDrag(PointerEventData pointerEvent)
    //{
    //    if (!pointerEvent.IsPointerMoving() || pointerEvent.pointerDrag == null)
    //        return;

    //    if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
    //    {
    //        ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
    //        pointerEvent.dragging = true;
    //    }

    //    if (pointerEvent.dragging)
    //    {
    //        if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
    //        {
    //            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

    //            pointerEvent.eligibleForClick = false;
    //            pointerEvent.pointerPress = null;
    //            pointerEvent.rawPointerPress = null;
    //        }
    //        ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
    //    }
    //}

    //private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
    //{
    //    if (!useDragThreshold)
    //        return true;

    //    return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
    //}
}