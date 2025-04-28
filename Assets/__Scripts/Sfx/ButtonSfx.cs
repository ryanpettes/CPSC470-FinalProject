using UnityEngine;
using UnityEngine.UI;

public class ButtonSfx : MonoBehaviour
{
    public Button button;  // Assign button in the Inspector
    public AudioClip clickSound;

    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        SfxManager.Instance.PlaySoundEffect(clickSound);
    }
}