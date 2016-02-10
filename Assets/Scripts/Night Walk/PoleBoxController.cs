﻿using UnityEngine;
using System.Collections;

public class PoleBoxController : MonoBehaviour {

    public GameObject poleBox;
    public GameObject innerBox;
    public GameObject positivePopup;
    public GameObject negativePopup;
    public ParticleSystem fireworks;
    public GameObject longPlatform;
    public GameObject mediumPlatform;
    public GameObject shortPlatform;

    private GameObject popup;
    private GameObject platform = null;
    private bool popped = false;
    private float popTime;

    public enum PlatformType
    {
        PLATFORM_NONE,
        PLATFORM_LONG,
        PLATFORM_MEDIUM,
        PLATFORM_SHORT
    }

    public enum PopType
    {
        POP_POSITIVE,
        POP_NEGATIVE,
        POP_EMPTY
    }

    public void Pop(PopType popType)
    {
        if (popType == PopType.POP_POSITIVE)
        {
            popup = positivePopup;
        }
        else if (popType == PopType.POP_NEGATIVE)
        {
            popup = negativePopup;
        }

        // Animations
        popped = true;
        popTime = Time.time;
        if (popType != PopType.POP_EMPTY)
        {
            popup.SetActive(true);
        }
        Animator animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsOpen", popped);

        // Fireworks
        innerBox.SetActive(false);
        fireworks.Play();
    }

    public void FixedUpdate()
    {
        if (popped)
        {
            // Popup movement
            float popupMultiplier = 13.0f;
            float popupInterpolant = Time.fixedDeltaTime * popupMultiplier;
            Vector3 fromPosition = popup.transform.position;
            Vector3 toPosition = transform.position + Vector3.up * 2.5f;
            popup.transform.position = Vector3.Lerp(
                fromPosition, toPosition, popupInterpolant);

            // Popup fading
            float fadingMultiplier = 3.0f;
            float timeDelta = Time.time - popTime;
            float fadingInterpolant = Mathf.Sin(timeDelta * fadingMultiplier);
            if (fadingInterpolant < 0)
            {
                enabled = false;
                fireworks.Stop();
            }

            Renderer renderer = popup.GetComponentInChildren<SpriteRenderer>();
            Color color = renderer.material.color;
            float alpha = Mathf.Lerp(color.a, 1.0f, popupInterpolant);
            color = new Color(color.r, color.g, color.b, fadingInterpolant);
            renderer.material.color = color;
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
        StartCoroutine(LowerCoroutine(amount));
    }

    public void DestroyPoleBox()
    {
        Destroy(gameObject);
    }

    private IEnumerator LowerCoroutine(float amount)
    {
        Vector3 fromPosition = transform.position;
        Vector3 toPosition = transform.position;
        toPosition.y -= amount;
        transform.position = toPosition;
        float startTime = Time.time;
        while (true)
        {
            float interpolant = (Time.time - startTime) * 5;
            transform.position = Vector3.Lerp(fromPosition, toPosition, interpolant);
            if (interpolant >= 1.0f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Done");
    }
}
