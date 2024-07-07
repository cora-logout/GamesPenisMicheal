using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMagia : MonoBehaviour
{
    [SerializeField] private int custo = 2;
    [SerializeField] private Text custoText;
    [SerializeField] private string cardName = "Default";
    [SerializeField] private Text nameText;
    [SerializeField] private string cardDescription = "Default";
    [SerializeField] private Text descriptionText;
    public bool cardBelongsToA = false;
    public bool canBeMovedM = false;
    public bool cardIsInHandM = false;
    public bool cardIsActiveM = false;
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
        if(!playerController.isInspecting && !cardIsActiveM)//only display outline if isn't inspecting and card isn't active
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
        custoText.text = custo.ToString();
        descriptionText.text = cardDescription;
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
        Dictionary<string, CardM> cardMagicDictionary = cardLibrary.cardMagicDictionary;
        List<string> MagicNames = new List<string>(cardMagicDictionary.Keys);
        //select a random index
        int randomIndex = Random.Range(0, MagicNames.Count);
        //get the random Magic name
        cardName = MagicNames[randomIndex];
        CardM card = cardMagicDictionary[cardName];
        string targetImagePath = card.PathToImageM;//get image path
        custo = card.Cost;
        cardDescription = card.Description;
        
        if (!string.IsNullOrEmpty(targetImagePath))//string != null
        {
            if (targetImagePath != null)
            {
                imageRenderer.material.mainTexture = Resources.Load<Texture>(targetImagePath);
            }
        }
        cardMagicDictionary.Remove(cardName);//remove card from dictionary
    }
}
