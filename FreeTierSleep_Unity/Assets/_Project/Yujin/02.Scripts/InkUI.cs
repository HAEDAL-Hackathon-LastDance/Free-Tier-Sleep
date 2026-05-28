using UnityEngine;
using UnityEngine.UI;

public class InkUI : MonoBehaviour
{
    [SerializeField] private Slider inkSlider;
    [SerializeField] private LineDrawer lineDrawer;

    void Update()
    {
        if (inkSlider == null || lineDrawer == null) return;

        float remaining = lineDrawer.MaxInk - lineDrawer.CurrentInk;
        inkSlider.value = remaining / lineDrawer.MaxInk;
    }
}
