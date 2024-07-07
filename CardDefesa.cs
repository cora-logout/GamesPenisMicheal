using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDefesa : MonoBehaviour
{
    [SerializeField] private int health = 20;
    [SerializeField] private Text healthText;
    [SerializeField] private string cardName = "Default";
    [SerializeField] private Text nameText;
    [SerializeField] private string description = "Default";
    [SerializeField] private Text descriptionText;
    public bool cardBelongsToA = false;
    public bool canBeMovedD = false;
    public bool cardIsInHandD = false;
    public bool cardIsActiveD = false;
    [SerializeField] private Renderer outlineRenderer;
    [SerializeField] private Renderer imageRenderer;
    private Color originalOutlineColor;
    private float defaultOutlineOpacity = 0f;
    private float hoverOutlineOpacity = 1f;
    private bool isHovering = false;
    private float mediumSmoothSpeed = 15f;
    private PlayerController playerController;
    private CardLibrary cardLibrary;
    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        cardLibrary = FindObjectOfType<CardLibrary>();
    }
    private void Start()
    {
        originalOutlineColor = outlineRenderer.material.color;
        SetOpacity(defaultOutlineOpacity);
        ChooseRandomCard();
    }
    void Update()
    {
        UpdateTexts();
        if(!playerController.isInspecting && !cardIsActiveD)//only display outline if isn't inspecting and card isn't active
        {
            Outline();
        }
        else
        {
            SetOpacity(defaultOutlineOpacity);
        }
    }
    private void OnMouseEnter()
    {
        isHovering = true;
    }
    private void OnMouseExit()
    {
        isHovering = false;
    }
    private void UpdateTexts()
    {
        this.gameObject.name = cardName;
        nameText.text = cardName;
        healthText.text = health.ToString();
        descriptionText.text = description;
    }
    private void SetOpacity(float opacity)
    {
        Color color = originalOutlineColor;
        color.a = opacity;
        outlineRenderer.material.color = color;
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
        Dictionary<string, CardD> cardDefenseDictionary = cardLibrary.cardDefenseDictionary;
        List<string> DefenseNames = new List<string>(cardDefenseDictionary.Keys);

        //select a random index
        int randomIndex = Random.Range(0, DefenseNames.Count);
        
        //get the random defense name
        cardName = DefenseNames[randomIndex];
        CardD card = cardDefenseDictionary[cardName];
        string targetImagePath = card.PathToImageD;//get image path
        if (!string.IsNullOrEmpty(targetImagePath))//string != null
        {
            if (targetImagePath != null)
            {
                imageRenderer.material.mainTexture = Resources.Load<Texture>(targetImagePath);
            }
        }
        cardDefenseDictionary.Remove(cardName);//remove card from dictionary
    }
}
