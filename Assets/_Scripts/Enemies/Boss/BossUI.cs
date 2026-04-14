using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public BossController boss;
    public Slider slider;

    void Update()
    {
        slider.value = boss.GetHPPercent();
        if (slider.value == 0)
        {
            Destroy(slider.gameObject);
        }
    }
}