﻿using UnityEngine;
using System.Collections;

public class PoleBoxController : MonoBehaviour {

    public GameObject poleBox;
    public GameObject innerBox;
    public GameObject goodPopup;
    public Transform popupGoodNode;
    public GameObject badPopup;
    public GameObject badPopupHeart;
    public GameObject badPopupWord;
    public GameObject missPopup;
    public GameObject missPopupNote;
    public GameObject missPopupDots;
    public GameObject missPopupCircle;
    public GameObject longPlatform;
    public GameObject mediumPlatform;
    public GameObject shortPlatform;
    public GameObject spinA;
    public GameObject spinB;
    public FireworksController fireworksController;

    private GameObject popup;
    private GameObject platform = null;

    public enum PlatformType
    {
        PLATFORM_NONE,
        PLATFORM_LONG,
        PLATFORM_MEDIUM,
        PLATFORM_SHORT
    }

    public enum PopType
    {
        POP_GOOD,
        POP_BAD,
        POP_MISS,
        POP_FIREWORKS,
        POP_EMPTY
    }

    public void Pop(PopType popType)
    {
        // Enable animations
        Animator animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsOpen", true);
        innerBox.SetActive(false);

        switch (popType)
        {
            case PopType.POP_GOOD:
                popup = goodPopup;
                bool rollFireworks = false;

                // Start animations
                fireworksController.StartFireworks(rollFireworks);
                StartCoroutine(YPositionCoroutine(popup, popupGoodNode, fireworksController.transform, 0.15f));
                StartCoroutine(AlphaCoroutine(popup, 1.0f, 0.0f, 1.0f, 0.0f));
                break;
            case PopType.POP_BAD:
                popup = badPopup;
                const float amountHeart = 0.8f;
                const float amountWord = 0.4f;
                fireworksController.DestroyFireworks();

                // Start animations
                StartCoroutine(YPositionCoroutine(badPopupHeart, popup.transform.position, amountHeart, 0.25f));
                StartCoroutine(YPositionCoroutine(badPopupWord, popup.transform.position, amountWord, 0.25f));
                StartCoroutine(AlphaCoroutine(badPopupHeart, 1.0f, 0.0f, 1.0f, 0.4f));
                StartCoroutine(AlphaCoroutine(badPopupWord, 1.0f, 0.0f, 0.1f, 0.4f));
                break;
            case PopType.POP_MISS:
                popup = missPopup;
                float amountY = 0.2f;
                fireworksController.DestroyFireworks();

                // Start animations
                StartCoroutine(YPositionCoroutine(missPopupNote, missPopupNote.transform.position, amountY, 0.15f));
                StartCoroutine(YPositionCoroutine(missPopupDots, missPopupDots.transform.position, amountY, 0.15f));
                StartCoroutine(AlphaCoroutine(missPopupNote, 0.0f, 1.0f, 0.1f, 0.0f));
                StartCoroutine(AlphaCoroutine(missPopupDots, 1.0f, 0.0f, 0.2f, 0.1f));
                StartCoroutine(AlphaCoroutine(missPopupCircle, 1.0f, 0.0f, 0.1f, 0.0f));
                break;
            case PopType.POP_FIREWORKS:
                RollFireworks();
                break;
            case PopType.POP_EMPTY:
                fireworksController.DestroyFireworks();
                break;
            default:
                Debug.Log("Unknown pop type " + popType);
                break;
        }

        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    public void SetPlatform(PlatformType type)
    {
        switch (type)
        {
            case PlatformType.PLATFORM_NONE:
                platform = null;
                break;
            case PlatformType.PLATFORM_LONG:
                platform = longPlatform;
                break;
            case PlatformType.PLATFORM_MEDIUM:
                platform = mediumPlatform;
                break;
            case PlatformType.PLATFORM_SHORT:
                platform = shortPlatform;
                break;
        }

        if (platform != null)
        {
            platform.SetActive(true);
        }
    }

    public void Lower(float amount)
    {
        StartCoroutine(YPositionCoroutine(gameObject, transform.position, -amount, 0.2f));
    }

    public void RollSpinEffects(bool first)
    {
        StartCoroutine(SpinCoroutine(first));
    }

    public void RollFireworks()
    {
        bool rollFireworks = true;
        fireworksController.StartFireworks(rollFireworks);
    }

    public void DestroyPoleBox()
    {
        if (popup != null)
        {
            StartCoroutine(AlphaCoroutine(popup, 1.0f, 0.0f, 1.0f, 0.0f));
        }
        StartCoroutine(AlphaCoroutine(poleBox, 1.0f, 0.0f, 1.0f, 0.0f));
        StartCoroutine(AlphaCoroutine(platform, 1.0f, 0.0f, 1.0f, 0.5f));
        StartCoroutine(DestroyPoleBoxCoroutine());
    }

    // TODO(jaween): Clean up this repeated code
    private IEnumerator SpinCoroutine(bool first)
    {
        GameObject gameObject = spinA;
        if (!first)
        {
            gameObject = spinB;
            yield return new WaitForSeconds(0.4f);
        }
        gameObject.SetActive(true);

        float startTime = Time.time;
        float interpolant = 0;
        Quaternion fromRotation = gameObject.transform.rotation;
        while (true)
        {
            float angle = first ? 360 : -360;
            Quaternion spin = Quaternion.AngleAxis(
                angle * interpolant, Vector3.forward);
            gameObject.transform.rotation = fromRotation * spin;

            Vector3 scale = gameObject.transform.localScale;
            scale = Vector3.one * (first ? (1 - interpolant) : interpolant);
            gameObject.transform.localScale = scale;

            // Fade
            SpriteRenderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            Color color = renderer.material.color;
            float fromAlpha = first ? 0.3f : 1.0f;
            float toAlpha = first ? 0.9f : 0.0f;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, interpolant);
            renderer.material.color = color;

            if (interpolant >= 1.0f)
            {
                break;
            }
            interpolant = (Time.time - startTime) * 3.0f;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
    
    // TODO(jaween): Make interfaces for alpha and yposition coroutines more similar
    private IEnumerator AlphaCoroutine(GameObject gameObject, float from, 
        float to, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float startTime = Time.time;
        float interpolant = 0;

        // TODO(jaween): What causes the NullReferenceException here?
        // It could be that the gameObject has been destroyed outside of this coroutine?
        // Solve the problem and remove the '&& gameObject != null' part
        while (true && gameObject != null)
        {
            // Fades in/out this GameObject and its children
            var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                Color color = renderer.material.color;
                if (color.a == 0 && to == 0)
                {
                    // If the child is already fully invisible when fading out don't make it visible
                    continue;
                }
                color.a = Mathf.Lerp(from, to, interpolant);
                renderer.material.color = color;
            }

            if (interpolant >= 1.0)
            {
                break;
            }
            interpolant = (Time.time - startTime) / duration;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator YPositionCoroutine(GameObject gameObject, Vector3 fromPosition,
    float amount, float duration)
    {
        Vector3 toPosition = fromPosition;
        toPosition.y += amount;

        float startTime = Time.time;
        float interpolant = 0;
        while (true)
        {
            gameObject.transform.position = Vector3.Lerp(fromPosition, toPosition, interpolant);
            if (interpolant >= 1.0)
            {
                break;
            }
            interpolant = (Time.time - startTime) / duration;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator YPositionCoroutine(GameObject gameObject, Transform from,
        Transform to, float duration)
    {
        float startTime = Time.time;
        float interpolant = 0;
        while (true)
        {
            gameObject.transform.position = Vector3.Lerp(from.position, to.position, interpolant);
            if (interpolant >= 1.0)
            {
                break;
            }
            interpolant = (Time.time - startTime) / duration;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DestroyPoleBoxCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
