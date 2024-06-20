using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLibrary : MonoBehaviour
{
    private static EffectLibrary _instance;
    private ScoreKeeper scoreKeeper;
    public Dictionary<string, Effect> effectDictionary;

    public static EffectLibrary Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EffectLibrary>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("EffectLibrary");
                    _instance = singletonObject.AddComponent<EffectLibrary>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        effectDictionary = new Dictionary<string, Effect>
        {
            { "Dude", new Effect { type = EffectType.Dude, effectValue = 0 } },
            { "HealI", new Effect { type = EffectType.Heal, effectValue = 10 } },
            { "HealII", new Effect { type = EffectType.Heal, effectValue = 15 } },
            { "HealIII", new Effect { type = EffectType.Heal, effectValue = 20 } },
            { "Hide", new Effect { type = EffectType.Hide, chance = 0.75f } },
            { "Lock", new Effect { type = EffectType.Lock, effectValue = 1 } },
            { "PoisonI", new Effect { type = EffectType.Poison, effectValue = 10, duration = 1 } },
            { "PoisonII", new Effect { type = EffectType.Poison, effectValue = 15, duration = 1 } },
            { "PoisonIII", new Effect { type = EffectType.Poison, effectValue = 10, duration = 2 } },
            { "Relax", new Effect { type = EffectType.Relax } },
            { "ScareI", new Effect { type = EffectType.Scare, effectValue = 10, duration = 1 } },
            { "ScareII", new Effect { type = EffectType.Scare, effectValue = 10, duration = 2, subsequentValue = 5 } },
            { "ScareIII", new Effect { type = EffectType.Scare, effectValue = 15, duration = 1, subsequentValue = 10 } }
        };
    }

    void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
    }

    public void ApplyEffect(string type)
    {
        bool AorB = scoreKeeper.AorB;
        if (effectDictionary.ContainsKey(type))
        {
            var effect = effectDictionary[type];
            switch (effect.type)
            {
                case EffectType.Dude:
                    scoreKeeper.DudePopUp();
                    break;
                case EffectType.Heal:
                    if (AorB)
                    {
                        scoreKeeper.playerAHealth += effect.effectValue;
                    }
                    else
                    {
                        scoreKeeper.playerBHealth += effect.effectValue;
                    }
                    Debug.Log("Healed " + effect.effectValue + " HP");
                    break;
                case EffectType.Hide:
                    Debug.Log("Character has a " + (effect.effectValue * 100) + "% chance to evade attacks");
                    break;
                case EffectType.Lock:
                    Debug.Log("Locked opponent's character for " + effect.effectValue + " turns");
                    break;
                case EffectType.Poison:
                    Debug.Log("Poisoned for " + effect.effectValue + " damage for " + effect.duration + " turns, ignoring defense");
                    break;
                case EffectType.Relax:
                    Debug.Log("Removed 'Scare' effect");
                    break;
                case EffectType.Scare:
                    if (effect.subsequentValue > 0)
                    {
                        Debug.Log("Scared: Enemy attack reduced by " + effect.effectValue + " for " + effect.duration + " turn(s), and by " + effect.subsequentValue + " for the subsequent turn(s)");
                    }
                    else
                    {
                        Debug.Log("Scared: Enemy attack reduced by " + effect.effectValue + " for " + effect.duration + " turn(s)");
                    }
                    break;
                default:
                    Debug.LogWarning("Effect type not handled: " + effect.type);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Effect not found: " + type);
        }
    }
}
public class Effect
{
    public EffectType type;
    public int effectValue;
    public float chance;
    public int duration;
    public int subsequentValue;
}

public enum EffectType
{
    Dude,
    Heal,
    Hide,
    Lock,
    Poison,
    Relax,
    Scare
}


