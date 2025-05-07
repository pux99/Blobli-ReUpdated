using System.Linq;
using UnityEngine;

public class Gem 
{
    public GameObject GameObject { get; private set; }
    public SpriteRenderer Renderer { get; private set; }
    public GemType Type { get; set; }

    public Gem(GameObject gameObject)
    {
        GameObject = gameObject;
        Renderer = gameObject.GetComponent<SpriteRenderer>();
        
    }
    public void setSprite(GemSpritePair pair) 
    {
        Renderer.sprite = pair.keyValuePairs.FirstOrDefault(x => x.Value == Type).Key;
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
    None,
    Shapire,
    Emerald,
    Quartz,
    KeyGem,
}