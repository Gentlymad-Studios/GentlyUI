using UnityEngine;

namespace GentlyUI.UIElements {
    public class GMDragDummy : MonoBehaviour
    {
        private GMDraggable origin;
        public GMDraggable Origin => origin;

        public void SetOrigin(GMDraggable origin) {
            this.origin = origin;
        }
    }
}
