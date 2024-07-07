using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CardPersonagem : MonoBehaviour
{
    private Dictionary<string, int> skillTagToNumber;
    private int health = 20;
    public Text healthText;
    private string cardName;
    public Text nameText;
    private string skill1Description;
    public Text skill1DescriptionText;
    private string skill2Description;
    public Text skill2DescriptionText;
    private string skill3Description;
    public Text skill3DescriptionText;
    private int skill1Cost = 0;
    public Text skill1CostText;
    private int skill2Cost = 0;
    public Text skill2CostText;
    private int skill3Cost = 0;
    public Text skill3CostText;
    public bool canBeMovedP = false;
    public bool cardBelongsToA = false;
    public bool cardIsInHandP = false;
    public bool cardIsActiveP = false;
    [SerializeField] private GameObject skillButtons;
    [SerializeField] private Renderer outlineRenderer;
    private Color originalOutlineColor;
    [SerializeField] private Renderer imageRenderer;
    private float defaultOutlineOpacity = 0f;
    private float hoverOutlineOpacity = 1f;
    private bool isHovering = false;
    private float mediumSmoothSpeed = 15f;
    private BoxCollider cardCollider;
    private Rigidbody rb;
    private CardLibrary cardLibrary;
    private ScoreKeeper scoreKeeper;
    private PlayerController playerController;
    private void Awake()
    {
        cardLibrary = FindObjectOfType<CardLibrary>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        playerController = FindObjectOfType<PlayerController>();
        cardCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
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
        
        originalOutlineColor = outlineRenderer.material.color;
        SetOpacity(defaultOutlineOpacity);
        ChooseRandomCard();
    }
    void Update()
    {
        UpdateTexts();
        if(cardIsActiveP && scoreKeeper.playerACanAttack)
        {
            DetectSkillClicks();
        }
        cardCollider.enabled = canBeMovedP;
        if(cardIsActiveP)
        {
            StartCoroutine(MakeCardKinematic());
        }
        if(!playerController.isInspecting)//only display outline if isn't inspecting
        {
            Outline();
        }
        else
        {
            SetOpacity(defaultOutlineOpacity);
        }
        HealthControl();
    }
    private void OnMouseEnter()
    {
        isHovering = true;
    }
    private void OnMouseExit()
    {
        isHovering = false;
    }
    private void SetOpacity(float opacity)
    {
        Color color = originalOutlineColor;
        color.a = opacity;
        outlineRenderer.material.color = color;
    }
    private void UpdateTexts()
    {
        this.gameObject.name = cardName;
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
        skillButtons.SetActive(cardIsActiveP);

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("SkillButton"))
            {
                SkillButton skillButton = hit.collider.GetComponent<SkillButton>();
                if (skillButton != null && cardName == skillButton.cardName && skillButton.isActive)
                {
                    cardLibrary.CardPSkill(skillButton.cardName, skillButton.skillNumber - 1);
                    skillButton.SetCooldown();//set current cooldown to the cooldown value for that skill
                    skillButton.Cooldown();
                }
            }
        }
    }
    private void HealthControl()
    {
        if(health <= 0)
        {
            SoundEffects soundEffects = FindObjectOfType<SoundEffects>();
            if (soundEffects != null)
            {
                soundEffects.PlayCardSound(cardName + "Death");
            }
            Destroy(this.gameObject);
        }
    }
    private void Outline()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform)
            {
                isHovering = true;
            }
            else
            {
                isHovering = false;
            }
        }
        else
        {
            isHovering = false;
        }

        if (isHovering)
        {
            float targetOpacity = Mathf.Lerp(outlineRenderer.material.color.a, hoverOutlineOpacity, Time.deltaTime * mediumSmoothSpeed);
            SetOpacity(targetOpacity);
        }
        else
        {
            float targetOpacity = Mathf.Lerp(outlineRenderer.material.color.a, defaultOutlineOpacity, Time.deltaTime * mediumSmoothSpeed);
            SetOpacity(targetOpacity);
        }
    }
    private void ChooseRandomCard()
    {
        //get all the keys from the dictionary
        Dictionary<string, CardP> cardCharacterDictionary = cardLibrary.cardCharacterDictionary;
        List<string> characterNames = new List<string>(cardCharacterDictionary.Keys);

        //select a random index
        int randomIndex = Random.Range(0, characterNames.Count);
        
        //get the random character name
        cardName = characterNames[randomIndex];
        CardP card = cardCharacterDictionary[cardName];
        health = card.Health;
        int skillIndex = 0;
        foreach (var skills in card.Skills)//go trough each skill and set their costs and descriptions
        {
            if (skillIndex == 0) 
            {
                skill1Cost = skills.Value.Cost;
            }
            else if (skillIndex == 1) 
            {
                skill2Cost = skills.Value.Cost;
            }
            else if (skillIndex == 2) 
            {
                skill3Cost = skills.Value.Cost;
            }
            skillIndex++;
        }
        string targetImagePath = card.PathToImageP;//get image path
        if (!string.IsNullOrEmpty(targetImagePath))//string != null
        {
            if (targetImagePath != null)
            {
                imageRenderer.material.mainTexture = Resources.Load<Texture>(targetImagePath);
            }
        }
        cardCharacterDictionary.Remove(cardName);//remove card from dictionary
    }
    private IEnumerator MakeCardKinematic()
    {
        yield return new WaitForSeconds(0.1f);
        rb.isKinematic = true;
    }
}
