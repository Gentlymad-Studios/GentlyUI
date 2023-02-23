using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.Core {
    public class UIObjectPool<T> where T : Behaviour {
        List<T> freeInstances = new List<T>();
        List<T> usedInstances = new List<T>();

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
        public T Get(Transform parent) {
            T _instance;

            if (FreeCount > 0) {
                _instance = freeInstances[FreeCount - 1];
                freeInstances.RemoveAt(FreeCount - 1);
            } else {
                _instance = create();
                _instance.transform.hierarchyCapacity = UIManager.UISettings.MaxHierarchyCapacity;

                if (!_instance.gameObject.activeInHierarchy) {
                    UIBase uiBase = _instance.gameObject.GetComponent<UIBase>();
                    if (uiBase != null) {
                        uiBase.InitializeUI();
                    }
                }

                if (_instance is IPooledUIResetter resetter) {
                    resetter.CreatePooledUICache();
                }

                allCount += 1;
            }

            usedInstances.Add(_instance);
            activeCount += 1;

            //Reparent if not in correct parent yet
            if (_instance.transform.parent != parent) _instance.transform.SetParent(parent, false);

            //Update transform
            _instance.transform.localScale = Vector3.one;
            //Make sure object sits at z-position 0
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.identity;
            //Activate if not active yet
            if (!_instance.gameObject.activeSelf) _instance.gameObject.SetActive(true);

            onGet?.Invoke(_instance);

            return _instance;
        }

        /// <summary>
        /// Return an instance to the pool
        /// </summary>
        /// <param name="instance"></param>
        public void Return(T instance) {
            if (usedInstances.Contains(instance)) {
                usedInstances.Remove(instance);
                freeInstances.Add(instance);
                activeCount -= 1;

                onReturn?.Invoke(instance);
                instance.transform.SetParent(null, true);
                instance.gameObject.SetActive(false);

                if (instance is IPooledUIResetter resetter) {
                    resetter.ResetPooledUI();
                }
            }
        }

        /// <summary>
        /// Returns all instances
        /// </summary>
        public void ReturnAll() {
            while (usedInstances.Count > 0) {
                Return(usedInstances[0]);
            }
        }

        /// <summary>
        /// Disposes the pool. All instances will be destroyed.
        /// </summary>
        public void Dispose() {
            for (int i = 0; i < activeCount; ++i) {
                GameObject.Destroy(usedInstances[i].gameObject);
            }

            for (int i = 0; i < FreeCount - activeCount; ++i) {
                GameObject.Destroy(freeInstances[i].gameObject);
            }

            freeInstances.Clear();
            usedInstances.Clear();

            activeCount = 0;
            allCount = 0;
        }

        /// <summary>
        /// Returns all instances that are currently in use. Note: Do not change this list!
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllUsedInstances() {
            return usedInstances;
        }
    }
}
