using UnityEngine;
using UnityEngine.UI;

public static class Extensions {
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
        if (gameObject.TryGetComponent(out T t)) {
            return t;
        } else {
            return gameObject.AddComponent<T>();
        }
    }
}
