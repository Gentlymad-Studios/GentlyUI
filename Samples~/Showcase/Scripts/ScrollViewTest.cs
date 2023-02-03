using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrollViewTest : GentlyUI.UIElements.GMPooledScrollView {
    public int count = 1;
    public bool spawnOverTime = false;
    public float spawnDelay = 1f;
    List<int> dataList = new List<int>();

    protected override void Start() {
        base.Start();

        if (spawnOverTime) {
            StartCoroutine(SpawnOverTime());
        } else {
            for (int i = 0, count = this.count; i < count; ++i) {
                dataList.Add(i + 1);
            }
        }

        Initialize<ScrollViewTestItem>(dataList.Count, OnUpdateItem);
    }
    
    IEnumerator SpawnOverTime() {
        int _count = 0;
        while (_count < count) {
            dataList.Add(_count + 1);
            _count += 1;
            Initialize<ScrollViewTestItem>(dataList.Count, OnUpdateItem);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void OnUpdateItem(Behaviour item, int dataIndex) {
        ScrollViewTestItem testItem = (ScrollViewTestItem)item;
        testItem.label.SetText(dataList[dataIndex].ToString());
    }
}
