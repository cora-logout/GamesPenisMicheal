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
    public bool cardIsInHandP = false;
    public bool cardIsActive = false;
    [SerializeField] private GameObject skillButtons;
    [SerializeField] private GameObject outline;
    private Material material;
    private Color originalOutlineColor;
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

        Renderer renderer = outline.GetComponent<Renderer>();
        material = renderer.material;
        originalOutlineColor = material.color;
        SetOpacity(defaultOutlineOpacity);
    }
    void Update()
    {
        UpdateTexts();
        if(cardIsActive)
        {
            DetectSkillClicks();
        }
        cardCollider.enabled = canBeMovedP;
        if(cardIsActive)
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
        material.color = color;
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
        skillButtons.SetActive(cardIsActive);

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("SkillButton"))
            {
                SkillButton skillButton = hit.collider.GetComponent<SkillButton>();
                if (skillButton != null && cardName == skillButton.cardName)
                {
                    cardLibrary.CardPSkill(skillButton.cardName, skillButton.skillNumber - 1);
                    skillButton.currentCoolDown = skillButton.coolDown;
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
            float targetOpacity = Mathf.Lerp(material.color.a, hoverOutlineOpacity, Time.deltaTime * mediumSmoothSpeed);
            SetOpacity(targetOpacity);
        }
        else
        {
            float targetOpacity = Mathf.Lerp(material.color.a, defaultOutlineOpacity, Time.deltaTime * mediumSmoothSpeed);
            SetOpacity(targetOpacity);
        }
    }
    private IEnumerator MakeCardKinematic()
    {
        yield return new WaitForSeconds(0.1f);
        rb.isKinematic = true;
    }
}
