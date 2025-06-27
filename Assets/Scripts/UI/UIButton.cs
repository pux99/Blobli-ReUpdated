using UnityEngine.UI;
using System;

public class UIButton
{
    public Button ButtonComponent { get; private set; }
    public int LevelIndex { get; private set; }

    public UIButton(Button button, int index)
    {
        ButtonComponent = button;
        LevelIndex = index;
    }

    public void Subscribe(Action<int> callback)
    {
        ButtonComponent.onClick.AddListener(() => callback(LevelIndex));
    }
}
