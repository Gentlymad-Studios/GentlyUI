using UnityEngine;
using TMPro;

namespace GentlyUI.UIElements {
    public class GMHeader : MonoBehaviour
    {
        public TextMeshProUGUI titleOutput;

        public void SetTitle(string title) {
            titleOutput.SetText(title);
        }
    }
}
