using GentlyUI.ModularUI;
using GentlyUI.UIElements;
using System.Collections;
using UnityEngine;

public class UISpawnTest : MonoBehaviour
{
    private void Start() {
        RectTransform container = UIContainerSpawner.CreateAnchoredRootUIContainer(
            "Main",
            "inspector",
            UIContainerSpawner.Anchor.BottomRight,
            500,
            -1,
            new Vector2(-25, 25)
        )
        .ApplyLayout(ContainerExtensions.Layout.Vertical, TextAnchor.MiddleCenter, 0f, new RectOffset(0, 0, 0, 0))
        .ApplyContentSizeFitter(UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize)
        .AddAnimationPresetForAnchor(UIContainerSpawner.Anchor.BottomRight);

        UIDefinitionSpawner.SpawnDefinition(container, typeof(UIDef_Inspector));

        StartCoroutine(ShowContainerDelayed(container));

        RectTransform window = UIContainerSpawner.CreateVerticallyStretchedRootUIContainer("Main", "window", 25f, 25, (1920f - 50f), UIContainerSpawner.Anchor.Center).ApplyLayout(ContainerExtensions.Layout.Horizontal, TextAnchor.MiddleCenter, 15f, new RectOffset(15, 15, 15, 15));

        RectTransform leftContent = UIContainerSpawner.CreateContainerInParent("left", window)
            .ApplyLayout(ContainerExtensions.Layout.Vertical, TextAnchor.MiddleCenter, 15f, new RectOffset(15, 15, 15, 15))
            .AddOrSetLayoutElement(-1, -1, 1);
        RectTransform centerContent = UIContainerSpawner.CreateContainerInParent("center", window)
            .ApplyLayout(ContainerExtensions.Layout.Vertical, TextAnchor.MiddleCenter, 15f, new RectOffset(15, 15, 15, 15))
            .AddOrSetLayoutElement(-1, -1, 2);
        RectTransform rightContent = UIContainerSpawner.CreateContainerInParent("right", window)
            .ApplyLayout(ContainerExtensions.Layout.Vertical, TextAnchor.MiddleCenter, 15f, new RectOffset(15, 15, 15, 15))
            .AddOrSetLayoutElement(-1, -1, 1);
    }

    IEnumerator ShowContainerDelayed(RectTransform container) {
        yield return new WaitForSeconds(1f);
        GMAnimatedContainer animC = container.GetComponent<GMAnimatedContainer>();
        animC.OnShow += () => Debug.Log("Show");
        animC.ShowContainer();
    }
}
