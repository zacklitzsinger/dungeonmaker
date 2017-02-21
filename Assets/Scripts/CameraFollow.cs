using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.main.Open)
            return;

        if (LevelEditor.main.mode >= EditMode.Create && !EventSystem.current.IsFieldFocused())
        {
            StopAllCoroutines();
            Vector3 motion = Input.GetAxisRaw("Horizontal") * Vector2.right + Input.GetAxisRaw("Vertical") * Vector2.up;
            motion /= Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 2 : 6;
            Rect bounds = LevelEditor.main.navmap.Bounds;
            bounds.position -= Vector2.one * 10;
            bounds.size += Vector2.one * 20;
            Vector3 pos = transform.position + motion;
            if (pos.x < bounds.xMin)
                pos.x = bounds.xMin;
            else if (pos.x > bounds.xMax)
                pos.x = bounds.xMax;
            if (pos.y < bounds.yMin)
                pos.y = bounds.yMin;
            else if (pos.y > bounds.yMax)
                pos.y = bounds.yMax;
            transform.position = pos;
        }
    }

    /// <summary>
    /// Starts easing the camera towards the target.
    /// </summary>
    public void SetTarget(Vector2 target)
    {
        StartCoroutine(EaseTowardsTarget(target, 0.5f));
    }

    IEnumerator EaseTowardsTarget(Vector2 targetPos, float duration)
    {
        Vector2 startPos = transform.position;
        float startTime = Time.time;
        float time = 0;
        while (time < duration)
        {
            yield return new WaitForEndOfFrame();
            time = Time.time - startTime;
            Vector3 newPosition = QuadEaseInOut(time, startPos, targetPos - startPos, duration);
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Quadratic ease in/out
    /// </summary>
    /// <param name="t">Time</param>
    /// <param name="b">Start value</param>
    /// <param name="c">Change in value</param>
    /// <param name="d">Duration</param>
    /// <returns></returns>
    Vector2 QuadEaseInOut (float t, Vector2 b, Vector2 c, float d)
    {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t-2) - 1) + b;
    }
}
