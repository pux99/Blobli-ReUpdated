using UnityEngine.UI;
using System;

public class UIButton
{
    public Button ButtonComponent { get; private set; }
    public string Scene { get; private set; }

    public UIButton(Button button, string sceneName)
    {
        ButtonComponent = button;
        Scene = sceneName;
    }

    public void Subscribe(Action<string> callback)
    {
        ButtonComponent.onClick.AddListener(() => callback(Scene));
    }
}
