using System;
using TMPro;
using UnityEngine;
using Utilities.MonoManager;

public class TextBlinker : MonoBehaviour, IUpdatable
{
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private float blinkSpeed = 1f;
    private Color originalColor;
    [SerializeField] private CustomMonoManager _customMonoManager;

    private void Awake()
    {
        _customMonoManager.RegisterOnUpdate(this);
    }

    private void Start()
    {
        originalColor = tmpText.color;
    }
    public void Tick(float deltaTime)
    {
        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}
