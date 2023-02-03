using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPooledUIResetter
{
    /// <summary>
    /// Called from object pool when an instance of the UI is instantiated.
    /// Use this method to cache default values. Make sure to apply them in ResetPooledUI().
    /// </summary>
    public abstract void CreatePooledUICache();

    /// <summary>
    /// Called from object pool when the UI is returned.
    /// Use this method to reset the UI object to its default values.
    /// </summary>
    public abstract void ResetPooledUI();
}
