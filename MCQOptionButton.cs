using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MCQOptionButton : MonoBehaviour
{
    public TextMeshProUGUI optionText;
    public Button button;

    int optionIndex;
    MCQManager manager;

    public void Setup(string text, int index, MCQManager mgr)
    {
        optionText.text = text;
        optionIndex = index;
        manager = mgr;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        manager.SubmitAnswer(optionIndex);
    }
}
