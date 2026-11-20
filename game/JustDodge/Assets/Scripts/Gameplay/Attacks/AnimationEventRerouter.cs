using UnityEngine;
using UnityEngine.Events;

public class AnimationEventRerouter : MonoBehaviour
{
    public UnityEvent OnAnimationEvent = new UnityEvent();

    public void AnimationEventTrigger()
    {
        OnAnimationEvent.Invoke();
    }
}
