using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CardPersonagem : MonoBehaviour
{
    private Dictionary<string, int> skillTagToNumber;
    public int health = 30;
    public Text healthText;
    public string cardName = "Default";
    public Text nameText;
    public string skill1Description = "Descrição detalhada sobre como funciona essa habilidade.";
    public Text skill1DescriptionText;
    public string skill2Description = "Default";
    public Text skill2DescriptionText;
    public string skill3Description = "Default";
    public Text skill3DescriptionText;
    public int skill1Cost = 0;
    public Text skill1CostText;
    public int skill2Cost = 0;
    public Text skill2CostText;
    public int skill3Cost = 0;
    public Text skill3CostText;
    public bool canBeMovedP = false;
    public bool cardBelongsToA = false;
    [SerializeField] private GameObject skillButtons;
    private CardLibrary cardLibrary;
    private ScoreKeeper scoreKeeper;
    private void Awake()
    {
        cardLibrary = FindObjectOfType<CardLibrary>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        //Check who just bought the card
        if(scoreKeeper.playerACanPlay)
        {
            cardBelongsToA = true;
        }
        else if(scoreKeeper.playerBCanPlay)
        {
            cardBelongsToA = false;
        }
    }
    private void Start()
    {
        skillTagToNumber = new Dictionary<string, int>
        {
            { "Skill1", 1 },
            { "Skill2", 2 },
            { "Skill3", 3 }
        };
    }
    void Update()
    {
        UpdateTexts();
        DetectSkillClicks();
    }
    private void UpdateTexts()
    {
        nameText.text = cardName;
        healthText.text = health.ToString();
        skill1DescriptionText.text = skill1Description;
        skill1CostText.text = skill1Cost.ToString();
        skill2DescriptionText.text = skill2Description;
        skill2CostText.text = skill2Cost.ToString();
        skill3DescriptionText.text = skill3Description;
        skill3CostText.text = skill3Cost.ToString();
    }
    private void DetectSkillClicks()
    {
        if(cardBelongsToA)
        {
            skillButtons.SetActive(scoreKeeper.playerACanAttack);
        }
        if(!cardBelongsToA)
        {
            skillButtons.SetActive(scoreKeeper.playerBCanAttack);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                SkillButton skillButton = hit.collider.GetComponentInChildren<SkillButton>();
                if (skillButton != null)
                {
                    cardLibrary.CardPSkill(skillButton.cardName, skillButton.skillNumber - 1);
                }
            }
        }
    }
}
