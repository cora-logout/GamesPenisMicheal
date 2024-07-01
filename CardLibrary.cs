using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FMODUnity;
using FMOD.Studio;

public class CardLibrary : MonoBehaviour
{
    private static CardLibrary _instance;
    public Dictionary<string, CardP> cardCharacterDictionary;
    public Dictionary<string, CardD> cardDefenseDictionary;
    public Dictionary<string, CardM> cardMagicDictionary;
    private ScoreKeeper scoreKeeper;
    private EffectLibrary effectLibrary;
    private PlayerController playerController;
    private string NotEnoughCoinsSound = "event:/NotEnoughEnergy";
    private FMOD.Studio.EventInstance notEnoughInstance;
    private string SpendCoinsSound = "event:/SpendCoins";
    private FMOD.Studio.EventInstance spendCoinsInstance;

    public static CardLibrary Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CardLibrary>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("CardLibrary");
                    _instance = singletonObject.AddComponent<CardLibrary>();
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
        cardCharacterDictionary = new Dictionary<string, CardP>
        {
            { "Testonildo", //Name of character
                new CardP("Testonildo", //Name of card
                new Dictionary<string, Skill> { //Dictionary for Skills
                    { "Golpe 1", new Skill(10, 1, "Default") }, //Skill name, Damage and Cost
                    { "Golpe 2", new Skill(20, 2, "Default") },
                    { "Golpe 3", new Skill(30, 3, "Default") }
                }) 
            },
            { "Bufos Regularis",
                new CardP("Bufos Regularis",
                new Dictionary<string, Skill> {
                    { "Ribbit", new Skill(15, 1, "Default") }, 
                    { "Salto", new Skill(10, 2, "Default") },
                    { "Bufada", new Skill(20, 3, "Default") }
                }) 
            },
            { "Billy Feijão do Mal", 
                new CardP("Billy Feijão do Mal",
                new Dictionary<string, Skill> {
                    { "Mordida", new Skill(10, 2, "Default") },
                    { "Golpe Terroso", new Skill(5, 1, "Default") },
                    { "Feijoada", new Skill(30, 4, "Default") }
                })
            },
            { "Cachilda", 
                new CardP("Cachilda",
                new Dictionary<string, Skill> {
                    { "Mordida", new Skill(10, 1, "Default") },
                    { "Latir", new Skill(0, 1, "ScareI") },
                    { "Camuflar", new Skill(0, 3, "Hide") }
                })
            },
            { "Chapéu Fumante", 
                new CardP("Chapéu Fumante",
                new Dictionary<string, Skill> {
                    { "Bomba de Fumaça", new Skill(15, 3, "Hide") },
                    { "Soco Simples", new Skill(10, 1, "Default") },
                    { "Chutar a Mesa", new Skill(10, 3, "KickTable") }
                })
            },
            { "Doutor Afanasfávio",
                new CardP("Doutor Afanasfávio",
                new Dictionary<string, Skill> {
                    { "Pílulas Mágicas", new Skill(0, 2, "HealI") },
                    { "Análise", new Skill(15, 2, "Dude") },
                    { "Pranchetada", new Skill(20, 2, "Default") }
                })
            },
            { "Fantasma", 
                new CardP("Fantasma", 
                new Dictionary<string, Skill> { 
                    { "Golpe Ectoplásmico", new Skill(10, 1, "Default") }, 
                    { "Grito do Abismo", new Skill(5, 1, "Default") },
                    { "Desaparecer", new Skill(0, 2, "Hide") } 
                }) 
            },
            { "João Bobão cara de Melão",
                new CardP("João Bobão cara de Melão",
                new Dictionary<string, Skill> {
                    { "Soco Simples", new Skill(10, 1, "Default") },
                    { "Uhhh", new Skill(30, 3, "Default") },
                    { "Eu tenho skins raras no jogo e eu posso mostrar pra você se você quiser e sua mãe deixar a minha deixa mas tem que ver com a sua", new Skill(10, 1, "Default") }
                })
            },
            { "Kary 233",
                new CardP("Kary 233",
                new Dictionary<string, Skill> {
                    { "Ataque Cibernético", new Skill(5, 1, "Hack") },
                    { "Ataque 2", new Skill(10, 1, "Default") },
                    { "Ataque 3", new Skill(10, 1, "Default") }
                })
            },
            { "Passarinho",
                new CardP("Passarinho", 
                new Dictionary<string, Skill> {
                    { "Bicada", new Skill(10, 1, "Default") },
                    { "Voar", new Skill(0, 2, "Fly") },
                    { "Fazer Cocô", new Skill(0, 2, "VisionLoss") }
                })
            }
        };
        cardDefenseDictionary = new Dictionary<string, CardD>
        {
            { "Caixa de Som", new CardD("Caixa de Som", 10, "Default") },
            { "Casaco de Frio", new CardD("Casaco de Frio", 15, "IncreaseSnowChance") },
            { "Guarda Chuva", new CardD("Guarda Chuva", 15, "ChuvaDef") },
            { "Pistola Globeriana V30", new CardD("Pistola Globeriana V30", 10, "Default") }
        };
        cardMagicDictionary = new Dictionary<string, CardM>
        {
            { "Pote de Picles", new CardM("Pote de Picles", 0, "Lock", 1) },
            { "Abdução de Cartas", new CardM("Abdução de Cartas", 0, "StealRandomCard", 0)}
        };
    }
    void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        effectLibrary = FindObjectOfType<EffectLibrary>();
        playerController = FindObjectOfType<PlayerController>();
        notEnoughInstance = RuntimeManager.CreateInstance(NotEnoughCoinsSound);
        spendCoinsInstance = RuntimeManager.CreateInstance(SpendCoinsSound);
    }
    public void CardPSkill(string cardName, int skillN)
    {
        if (cardCharacterDictionary.ContainsKey(cardName))//check for requested card
        {
            var card = cardCharacterDictionary[cardName];
            if (skillN >= 0 && skillN < card.Skills.Count)//check for specific skill
            {
                var skillEntry = card.Skills.ElementAt(skillN);
                string skillName = skillEntry.Key;
                Skill skill = skillEntry.Value;
                int damage = skill.Damage;
                int cost = skill.Cost;
                string type = skill.Type;

                if (scoreKeeper.playerACoins >= cost)
                {
                    spendCoinsInstance.start();
                    playerController.RemoveCoins(cost);
                    scoreKeeper.playerACoins -= cost;
                    SoundEffects soundEffects = FindObjectOfType<SoundEffects>();
                    soundEffects.PlayCardSound(cardName + "Attack");
                    Debug.Log("Used skill: " + skillName + ". With damage: " + damage + ". And cost: " + cost + ". And type: " + type);
                    scoreKeeper.DoDamage(damage);
                    playerController.StartCoroutine(playerController.SpentCoinsAnim());
                    if(type != "Default")
                    {
                        effectLibrary.ApplyEffect(type);
                    }
                }
                else
                {
                    notEnoughInstance.start();
                    playerController.StartCoroutine(playerController.NotEnoughCoinsAnim());
                }
            }
            else
            {
                Debug.LogWarning("Invalid skill number: " + skillN);
            }
        }
        else
        {
            Debug.LogWarning("Card not found.");
        }
    }
}
public class CardP
{
    public string Name { get; set; }
    public Dictionary<string, Skill> Skills;

    public CardP(string name, Dictionary<string, Skill> skills)
    {
        Name = name;
        Skills = skills;
    }
}
public class Skill
{
    public int Damage { get; set; }
    public int Cost { get; set; }
    public string Type { get; set; }

    public Skill(int damage, int cost, string type)
    {
        Damage = damage;
        Cost = cost;
        Type = type;
    }
}
public class CardD
{
    public string Name { get; set; }
    public int Health { get; set; }
    public string Effect { get; set; }

    public CardD(string name, int health, string effect)
    {
        Name = name;
        Health = health;
        Effect = effect;
    }
}
public class CardM
{
    public string Name { get; set; }//Name of card
    public int Damage { get; set; }//Damage cards deals (Optional)
    public string Effect1 { get; set; }//Effect card gives (Optional)
    public int Fx1Force { get; set; }//Force of Effect 1, if any

    public CardM(string name, int damage, string effect1, int fx1force)
    {
        Name = name;
        Damage = damage;
        Effect1 = effect1;
        Fx1Force = fx1force;
    }
}