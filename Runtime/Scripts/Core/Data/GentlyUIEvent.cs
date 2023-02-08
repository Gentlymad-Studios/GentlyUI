using UnityEngine.Events;

public class GentlyUIEvent : UnityEvent {
    private int listenerCount;

    public int ListenerCount { 
        get { 
            return listenerCount; 
        } 
    }

    public GentlyUIEvent() {
        listenerCount = 0;
    }

    new public void AddListener(UnityAction call) {
        base.AddListener(call);
        ++listenerCount;
    }

    new public void RemoveListener(UnityAction call) {
        base.RemoveListener(call);
        --listenerCount;
    }
}