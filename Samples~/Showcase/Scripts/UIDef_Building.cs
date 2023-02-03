using GentlyUI.ModularUI;
using GentlyUI.UIElements;
using System.Collections.Generic;
using UnityEngine;

public class UIDef_Building : UIDefinition {
    public UIDef_Building(RectTransform container, object data = null) : base(container, data) { }

    List<int> dataList = new List<int>();

    public override void CreateUI(object data = null) {
        ScrollViewTestItem testItem = UIPrefabLoader.LoadUIElement("testItem").GetComponent<ScrollViewTestItem>();

        //Test data
        if (dataList.Count == 0) {
            for (int i = 0, count = 25; i < count; ++i) {
                dataList.Add(i + 1);
            }
        }

        AddHeader("Building Definition");

        AddContent();
        {
            AddDropdown(0, (int value) => Debug.Log(value), new List<GentlyUI.UIElements.GMDropdown.DropdownOptionData>() {
                new GMDropdown.DropdownOptionData("Option 01"),
                new GMDropdown.DropdownOptionData("Option 02"),
                new GMDropdown.DropdownOptionData("Option 03"),
                new GMDropdown.DropdownOptionData("Option 04"),
                new GMDropdown.DropdownOptionData("Option 05"),
                new GMDropdown.DropdownOptionData("Option 06")
            });

            GMToggleGroup toggleGroup = AddToggleGroup();
            {

                AddToggle(true, (bool isOn) => {
                    Debug.Log(isOn);
                });

                AddToggle(false, (bool isOn) => {
                    Debug.Log(isOn);
                });

                AddToggle(false, (bool isOn) => {
                    Debug.Log(isOn);
                });
            }
            EndToggleGroup();

            toggleGroup.OnActiveToggleChanged.AddListener((GMToggle activeToggle) => {
                if (activeToggle != null) {
                    Debug.Log(activeToggle.name + ": " + activeToggle.IsOn);
                } else {
                    Debug.Log("No active toggle");
                }
            });

            AddButton(() => {
                UIDefinitionSpawner.SpawnDefinition<UIDef_Inspector>(rootContainer);
            }, "Open Generic Def");

            AddPooledScrollView<ScrollViewTestItem>(
                testItem,
                dataList,
                OnUpdateItem
            );

            AddContent_GridFixedColumns(3, 50f, Vector2.one * 5f, new RectOffset(15, 15 ,15 ,15));
            {

                AddButton(() => Debug.Log("Clicked"), "Upgrade");
                AddButton(() => Debug.Log("Clicked"), "Move AOI");
                AddButton(() => Debug.Log("Clicked"), "Dismantle");
            }
            EndContent();

        }
        EndContent();

        AddToggleGroup();
        {
            AddToggle(false, (bool isOn) => { }, "Toggle");
            AddToggle(false, (bool isOn) => { }, "Toggle");
            AddToggle(false, (bool isOn) => { }, "Toggle");
        }

        EndContent();

        AddFooter();
    }

    void OnUpdateItem(Behaviour item, int dataIndex) {
        ScrollViewTestItem testItem = (ScrollViewTestItem)item;
        testItem.label.text = dataIndex.ToString();
    }
}
