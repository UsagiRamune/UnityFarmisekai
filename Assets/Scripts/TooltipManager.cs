using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    public TextMeshProUGUI tooltipText;
    public GameObject tooltipUI;
    public bool suppressTooltip = false;
    private void Awake()
    {
        Instance = this;
        tooltipUI.SetActive(false);
    }

    public void Show(string itemName)
    {
        if (suppressTooltip)
        {
            Hide();
            return;
        }
        tooltipUI.SetActive(true);
        tooltipText.text = itemName;
    }

    public void Hide()
    {
        tooltipUI.SetActive(false);
    }
}