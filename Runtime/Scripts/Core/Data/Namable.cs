
using UnityEngine;

namespace GentlyUI.Core {
    public abstract class Namable : ISerializationCallbackReceiver {
        [HideInInspector]
        public string name;

        public void OnAfterDeserialize() {
            //UpdateName();
        }

        public void OnBeforeSerialize() {
            UpdateName();
        }

        protected void SetName(string name) {
            this.name = name;
        }

        public abstract void UpdateName();
    }
}