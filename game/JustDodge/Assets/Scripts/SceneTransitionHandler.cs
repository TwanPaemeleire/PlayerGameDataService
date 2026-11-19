using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MoveOffScreenEntry
{
    public Vector2 Direction;
    public RectTransform Transform;
    public CanvasGroup CanvasGroup;
}

public class SceneTransitionHandler : MonoBehaviour
{
    [SerializeField] private List<MoveOffScreenEntry> _entriesToMoveOffScreen = new List<MoveOffScreenEntry>();
    [SerializeField] private float _delayBeforeTransitionStart = 0.0f;
    [SerializeField] private float _delayBeforeFadeStart = 0.2f;
    [SerializeField] private float _timeToFadeOut = 0.5f;
    [SerializeField] private float _moveSpeed = 200;
    [SerializeField] private float _movementWindUpTime = 0.2f;

    public void RequestSceneTransition(string sceneName)
    {
        StartCoroutine(SceneTransition(sceneName));
    }

    private IEnumerator SceneTransition(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(_delayBeforeTransitionStart);
        Coroutine moveCoroutine = StartCoroutine(MoveEntries());
        yield return new WaitForSeconds(_delayBeforeFadeStart);
        yield return StartCoroutine(TransitionAnimation());
        StopCoroutine(moveCoroutine);

        yield return new WaitUntil(() => operation.progress >= 0.9f);
        operation.allowSceneActivation = true;
    }

    private IEnumerator MoveEntries()
    {
        float counter = 0.0f;
        while(true)
        {
            counter += Time.deltaTime;
            float speedToApply = Mathf.SmoothStep(0.0f, _moveSpeed, Mathf.Clamp01(counter / _movementWindUpTime));
            foreach (var entry in _entriesToMoveOffScreen)
            {
                entry.Transform.anchoredPosition += entry.Direction * speedToApply * Time.deltaTime;
            }
            yield return null;
        }
    }

    private IEnumerator TransitionAnimation()
    {
        float counter = 0.0f;
        while(counter < _timeToFadeOut)
        {
            counter += Time.deltaTime;
            float progress = counter / _timeToFadeOut;
            float smoothProgress = Mathf.SmoothStep(1.0f, 0.0f, progress);
            foreach(var entry in _entriesToMoveOffScreen)
            {
                entry.CanvasGroup.alpha = smoothProgress;
            }
            yield return null;
        };
    }
}
