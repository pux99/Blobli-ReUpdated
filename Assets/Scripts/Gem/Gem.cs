using UnityEngine;

public class Gem 
{
    public GameObject GameObject { get; private set; }
    public GemType Type { get; set; }

    public Gem(GameObject gameObject)
    {
        GameObject = gameObject;
    }

    public void Activate()
    {
        GameObject.SetActive(true);
    }

    public void Deactivate()
    {
        GameObject.SetActive(false);
    }
}
public enum GemType
{
    Shapire,
    Emerald,
    Quartz,
    KeyGem,
}