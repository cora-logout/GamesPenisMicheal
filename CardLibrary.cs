using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FMODUnity;
using FMOD.Studio;

/*Effects:
    - Heal (Cura) I. Heals 10 HP
    - Heal II. Heals 15 HP
    - Heal III. Heals 20 HP

    - Hide (Esconder). Affected character has a 75% chance to completely avoid attacks that require visual confirmation of target

    - Lock (Tranca) (int). Locks opponent's character for an int amount of turns.
    
    - Poison (Veneno) I. Deals 10 damage to character affected or player at the end of next turn, ignores defense.
    - Poison II. Deals 15 damage ..
    - Poison III. Deals 10 damage to character affected or player at the end of the next *two* turns, ignores defense.

    - Relax (Relaxar). Remove "Scare"

    - Scare (Assustar) I. Lowers enemy attack by 10 for the next turn
    - Scare II. Lowers enemy attack by 10 for the next turn and by 5 for the turn after that
    - Scare III. Lowers enemy attack by 15 for the next turn and by 10 for the turn after that
*/
public class CardLibrary : MonoBehaviour
{
    private static CardLibrary _instance;
    public Dictionary<string, CardP> cardCharacterDictionary;
    public Dictionary<string, CardD> cardDefenseDictionary;
    public Dictionary<string, CardM> cardMagicDictionary;
    private ScoreKeeper scoreKeeper;
    private PlayerController playerController;
    private string NotEnoughCoinsSound = "event:/NotEnoughEnergy";
    private FMOD.Studio.EventInstance notEnoughInstance;

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
                    { "Golpe 1", new Skill(10, 1) }, //Skill name, Damage and Cost
                    { "Golpe 2", new Skill(20, 2) },
                    { "Golpe 3", new Skill(30, 3) }
                }, "Neutral" ) 
            },
            { "Bufos Regularis",
                new CardP("Bufos Regularis",
                new Dictionary<string, Skill> {
                    { "Ribbit", new Skill(15, 1) }, 
                    { "Salto", new Skill(10, 2) },
                    { "Bufada", new Skill(20, 3) }
                }, "Neutral" ) 
            },
            { "Billy Feijão do Mal", 
                new CardP("Billy Feijão do Mal",
                new Dictionary<string, Skill> {
                    { "Mordida", new Skill(10, 2) },
                    { "Golpe Terroso", new Skill(5, 1) },
                    { "Feijoada", new Skill(30, 4) }
                }, "Neutral" )
            },
            { "Cachilda", 
                new CardP("Cachilda",
                new Dictionary<string, Skill> {
                    { "Mordida", new Skill(10, 2) },
                    { "Latir", new Skill(0, 1) /*ScareI*/},
                    { "Camuflar", new Skill(0, 3) /*Hide*/}
                }, "Neutral" )
            },
            { "Doutor Afanasfávio",
                new CardP("Doutor Afanasfávio",
                new Dictionary<string, Skill> {
                    { "Pílulas Mágicas", new Skill(0, 2) /*HealI*/},
                    { "Análise", new Skill(15, 2) },
                    { "Pranchetada", new Skill(20, 2) }
                }, "Neutral" )
            },
            { "Fantasma", 
                new CardP("Fantasma", 
                new Dictionary<string, Skill> { 
                    { "Golpe Ectoplásmico", new Skill(10, 1) }, 
                    { "Grito do Abismo", new Skill(5, 1) },
                    { "Desaparecer", new Skill(0, 2) } 
                }, "Ghost" ) 
            }
        };
        cardDefenseDictionary = new Dictionary<string, CardD>
        {
            { "Caixa de Som", new CardD("Caixa de Som", 10) }
        };
        cardMagicDictionary = new Dictionary<string, CardM>
        {
            { "Pote de Picles", new CardM("Pote de Picles", 0, "Lock", 1) }
        };
    }
    void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        playerController = FindObjectOfType<PlayerController>();
        notEnoughInstance = RuntimeManager.CreateInstance(NotEnoughCoinsSound);
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

                if (scoreKeeper.playerACoins >= cost)
                {
                    playerController.RemoveCoins(cost);
                    scoreKeeper.playerACoins -= cost;
                    SoundEffects soundEffects = FindObjectOfType<SoundEffects>();
                    soundEffects.PlayCardSound(cardName + "Attack");
                    Debug.Log("Used skill: " + skillName + ". With damage: " + damage + ". And cost: " + cost);
                    scoreKeeper.DoDamage(damage);
                }
                else
                {
                    notEnoughInstance.start();
                    Debug.Log("Not enough coins to use skill: " + skillName + ". Because it costs: " + cost + ". And playerA has: " + scoreKeeper.playerACoins + " coins.");
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
    public string Type { get; set; }

    public CardP(string name, Dictionary<string, Skill> skills, string type)
    {
        Name = name;
        Skills = skills;
        Type = type;
    }
}

public class Skill
{
    public int Damage { get; set; }
    public int Cost { get; set; }

    public Skill(int damage, int cost)
    {
        Damage = damage;
        Cost = cost;
    }
}

public class CardD
{
    public string Name { get; set; }
    public int Health { get; set; }

    public CardD(string name, int health)
    {
        Name = name;
        Health = health;
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