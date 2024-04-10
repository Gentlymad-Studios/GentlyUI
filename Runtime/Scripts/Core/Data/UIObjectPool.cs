using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.Core {
    public class UIObjectPool<T> where T : Behaviour {
        Stack<T> pooledObjects = new Stack<T>();

        Func<T> create = null;
        Action<T> onReturn = null;
        Action<T> onGet = null;

        private int activeCount = 0;
        public int ActiveCount => activeCount;

        private int allCount = 0;
        private int FreeCount => allCount - activeCount;

        /// <summary>
        /// Create a new Object Pool.
        /// createInstance = How an instance should be created. Returns the new instance
        /// onGetInstance = Optional action to apply to an instance when it is fetched from the pool
        /// onReturnInstance = Optional action to apply to an instance when it is returned to the pool
        /// </summary>
        /// <param name="create"></param>
        /// <param name="onReturn"></param>
        /// <param name="onGet"></param>
        public UIObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onReturn = null) {
            this.create = create;
            this.onGet = onGet;
            this.onReturn = onReturn;
        }

        /// <summary>
        /// Get an instance from the pool. If no free instance is available, a new one will be created.
        /// </summary>
        /// <returns></returns>
        public T Get(Transform parent, bool setActive = true) {
            T _instance;

            if (pooledObjects.Count > 0) {
                _instance = pooledObjects.Pop();

            } else {
                _instance = create();

                foreach (IPooledUIResetter resetter in _instance.GetComponents<IPooledUIResetter>()) {
                    resetter.CreatePooledUICache();
                }
            }

            //Reparent if not in correct parent yet
            if (_instance.transform.parent != parent) {
                _instance.transform.SetParent(parent, false);
            }

            //Update transform
            _instance.transform.localScale = Vector3.one;
            //Make sure object sits at z-position 0
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.identity;
            //Activate if not active yet
            if (_instance.gameObject.activeSelf != setActive) {
                _instance.gameObject.SetActive(setActive);
            }

            onGet?.Invoke(_instance);

            return _instance;
        }

        /// <summary>
        /// Return an instance to the pool
        /// </summary>
        /// <param name="instance"></param>
        public void Return(T instance) {
            pooledObjects.Push(instance);

            onReturn?.Invoke(instance);
            instance.transform.SetParent(null, true);
            instance.gameObject.SetActive(false);

            foreach (IPooledUIResetter resetter in instance.GetComponents<IPooledUIResetter>()) {
                resetter.ResetPooledUI();
            }
        }
    }
}
