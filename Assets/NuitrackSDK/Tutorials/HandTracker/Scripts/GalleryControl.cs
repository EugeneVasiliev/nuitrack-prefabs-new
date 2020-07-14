﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class GalleryControl : MonoBehaviour
{
    enum ViewMode { Preview, View };
    ViewMode currentViewMode = ViewMode.Preview;

    [Header("Visualization")]

    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Sprite[] spriteCollection;
    [SerializeField] RectTransform content;
    [SerializeField] GameObject imageItemPrefab;

    [SerializeField] CanvasGroup canvasGroup;

    [Header("Grid")]

    [Range(1, 10)]
    [SerializeField] int rowsNumber = 3;
    [Range(1, 10)]
    [SerializeField] int colsNumber = 4;

    Vector2 pageSize;
    int numberOfPages = 0;

    Vector2 defaultSize;   

    [Header("Scroll")]

    [Range(0.1f, 10)]
    [SerializeField] float scrollSpeed = 4f;

    float scrollStep = 0;

    [Header("View")]
    [SerializeField] RectTransform viewRect;

    Vector2 viewRectAnchor;
    Vector2 startRectSize;
    Vector2 startAnchorPosition;
    Quaternion startRotation;
    Vector2 startScale;

    [Range(0.1f, 16f)]
    [SerializeField] float animationSpeed = 2;
    [SerializeField] AnimationCurve animationCurve;

    ImageItem selectedItem = null;

    bool animated = false;
    float t = 0;
 
    int currentPage = 0;

    IEnumerator Start()
    {
        yield return null;

        pageSize = scrollRect.viewport.rect.size;
        defaultSize = new Vector2(pageSize.x / colsNumber, pageSize.y / rowsNumber);

        Vector2 halfAdd = new Vector2(defaultSize.x / 2, -defaultSize.y / 2);

        int imagesOnPage = rowsNumber * colsNumber;
        numberOfPages = (int)Mathf.Ceil((float)spriteCollection.Length / imagesOnPage);

        int imageIndex = 0;

        for (int p = 0; p < numberOfPages; p++)
        {
            int imagesOnCurrentPage = Mathf.Min(spriteCollection.Length - p * imagesOnPage, imagesOnPage);

            for (int i = 0; i < imagesOnCurrentPage; i++)
            {
                GameObject currentItem = Instantiate(imageItemPrefab);
                currentItem.transform.SetParent(content.transform, false);

                ImageItem currentImageItem = currentItem.GetComponent<ImageItem>();
                currentImageItem.Rect.sizeDelta = defaultSize;

                float X = pageSize.x * p + defaultSize.x * (i % colsNumber);
                float Y = defaultSize.y * (i / colsNumber);

                currentImageItem.Rect.anchoredPosition = new Vector2(X, -Y) + halfAdd;

                currentImageItem.image.sprite = spriteCollection[imageIndex];
                imageIndex++;

                currentImageItem.onClick.AddListener(delegate { CurrentImageItem_OnClick(currentImageItem); });
            }
        }

        content.sizeDelta = new Vector2(pageSize.x * numberOfPages, pageSize.y);

        if (numberOfPages > 1)
            scrollStep = 1f / (numberOfPages - 1);

        NuitrackManager.onNewGesture += NuitrackManager_onNewGesture;        
    }

    private void OnDestroy()
    {
        NuitrackManager.onNewGesture -= NuitrackManager_onNewGesture;
    }

    private void CurrentImageItem_OnClick(ImageItem currentItem)
    {
        if (currentViewMode == ViewMode.Preview && !animated)
        {
            t = 0;
            currentViewMode = ViewMode.View;
            selectedItem = currentItem;

            canvasGroup.interactable = false;
            //selectedItem.interactable = false;

            selectedItem.transform.SetParent(viewRect, true);

            startAnchorPosition = selectedItem.Rect.anchoredPosition;
            startRectSize = selectedItem.Rect.sizeDelta;
            viewRectAnchor = startAnchorPosition;
        }
    }

    private void Update()
    {
        switch (currentViewMode)
        {
            case ViewMode.View:

                if (t < 1)
                {
                    t += Time.deltaTime * animationSpeed;
                    float shift = animationCurve.Evaluate(t);

                    canvasGroup.alpha = Mathf.Lerp(1, 0, shift);

                    selectedItem.Rect.sizeDelta = Vector2.Lerp(startRectSize, pageSize, shift);
                    Vector2 pageAnchorPosition = new Vector2(pageSize.x / 2, -pageSize.y / 2);
                    selectedItem.Rect.anchoredPosition = Vector2.Lerp(startAnchorPosition, pageAnchorPosition, shift);
                }

                break;

            case ViewMode.Preview:

                if (animated)
                {
                    if (t < 1)
                    {
                        t += Time.deltaTime * animationSpeed;
                        float shift = animationCurve.Evaluate(t);

                        canvasGroup.alpha = Mathf.Lerp(0, 1, shift);

                        selectedItem.Rect.sizeDelta = Vector2.Lerp(startRectSize, defaultSize, shift);

                        selectedItem.Rect.anchoredPosition = Vector2.Lerp(startAnchorPosition, viewRectAnchor, shift);
                        selectedItem.Rect.localRotation = Quaternion.Lerp(startRotation, Quaternion.identity, shift);
                        selectedItem.Rect.localScale = Vector3.Lerp(startScale, Vector3.one, shift);
                    }
                    else
                    {
                        selectedItem.transform.SetParent(content, true);
                        //selectedItem.interactable = true;
                        canvasGroup.interactable = true;
                        selectedItem = null;
                        animated = false;
                    }
                }
                else
                    scrollRect.horizontalScrollbar.value = Mathf.Lerp(scrollRect.horizontalScrollbar.value, scrollStep * currentPage, Time.deltaTime * scrollSpeed);

                break;
        }
    }

    private void NuitrackManager_onNewGesture(nuitrack.Gesture gesture)
    {
        switch (currentViewMode)
        {
            case ViewMode.Preview:

                if (gesture.Type == nuitrack.GestureType.GestureSwipeLeft)
                    currentPage = Mathf.Clamp(++currentPage, 0, numberOfPages);

                if (gesture.Type == nuitrack.GestureType.GestureSwipeRight)
                    currentPage = Mathf.Clamp(--currentPage, 0, numberOfPages);

                break;

            case ViewMode.View:

                if (gesture.Type == nuitrack.GestureType.GestureSwipeUp)
                {
                    currentViewMode = ViewMode.Preview;
                    animated = true;
                    t = 0;

                    startRectSize = selectedItem.Rect.sizeDelta;

                    startAnchorPosition = selectedItem.Rect.anchoredPosition;
                    startRotation = selectedItem.Rect.localRotation;
                    startScale = selectedItem.Rect.localScale;
                }
                break;
        }
    }
}
