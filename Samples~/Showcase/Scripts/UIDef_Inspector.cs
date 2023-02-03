using GentlyUI;
using GentlyUI.ModularUI;
using GentlyUI.UIElements;
using System.Collections.Generic;
using UnityEngine;

public class UIDef_Inspector : UIDefinition {

    List<bool> dataList = new List<bool>();

    public UIDef_Inspector(RectTransform container, object data = null) : base(container, data) {}

    public override void CreateUI(object data = null) {
        GMToggle toggleItem = UIPrefabLoader.LoadUIElement("toggle").GetComponent<GMToggle>();

        //Test data
        if (dataList.Count == 0) {
            for (int i = 0, count = 50; i < count; ++i) {
                dataList.Add(false);
            }
        }

        AddHeader("Generic Inspector");

        AddContent();
            AddInputField("", (string text) => Debug.Log(text));

            AddButton(() => {
                UIDefinitionSpawner.SpawnDefinition<UIDef_Building>(rootContainer);
            }, "Open Building Def");

            AddButton(() => {
                rootContainer.GetComponent<GMAnimatedContainer>().HideContainer();
            }, "Close (Restart required)");

            AddPooledScrollView<GMToggle>(
                toggleItem, 
                dataList,
                OnUpdateItem,
                OnReturnItem
            );

            AddText("Toller Typ bist du!", TMPro.TextAlignmentOptions.MidlineRight);

            AddSlider(0, 0, 10, false, (float value) => { });

        EndContent();

        AddFooter();
    }

    void OnUpdateItem(Behaviour item, int dataIndex) {
        GMToggle toggle = item.GetComponent<GMToggle>();
        toggle.OnValueChanged.RemoveAllListeners();
        toggle.SetInitialValue(dataList[dataIndex]);
        toggle.OnValueChanged.AddListener((bool isOn) => dataList[dataIndex] = isOn);
    }

    void OnReturnItem(Behaviour item) {
        Debug.Log("Returned " + item.name);
    }
}
