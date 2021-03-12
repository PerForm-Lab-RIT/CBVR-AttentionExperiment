using ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

/*
 * Class: GameEventListener
 *
 * A generic class that provides event listening behavior to any GameObject
 * Meant to be used in tandem with instances of the GameEvent scriptable object.
 *
 * See the TrialManager GameObject to see how it's used
 */
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public UnityEvent response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    public void RaiseEvent()
    {
        response.Invoke();
    }
}
