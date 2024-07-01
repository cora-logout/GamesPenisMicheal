using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD;
using FMOD.Studio;

public class CardMagia : MonoBehaviour
{
    [SerializeField] private int custo = 2;
    [SerializeField] private Text custoText;
    [SerializeField] private new string name = "Default";
    [SerializeField] private Text nameText;
    [SerializeField] private string cardDescription = "Default";
    [SerializeField] private Text descriptionText;
    public bool cardBelongsToA = false;
    public bool canBeMovedM = false;
    public bool cardIsInHandM = false;
    [SerializeField] private GameObject outline;
    private Material material;
    private Color originalOutlineColor;
    private float defaultOutlineOpacity = 0f;
    private float hoverOutlineOpacity = 1f;
    private bool isHovering = false;
    private float mediumSmoothSpeed = 15f;
    private PlayerController playerController;
    void Awake()
    {
        nameText.text = name;
        custoText.text = custo.ToString();
        descriptionText.text = cardDescription;
        playerController = FindObjectOfType<PlayerController>();
    }
    private void Start()
    {
        Renderer renderer = outline.GetComponent<Renderer>();
        material = renderer.material;
        originalOutlineColor = material.color;
        SetOpacity(defaultOutlineOpacity);
    }
    void Update()
    {
        UpdateTexts();
        if(!playerController.isInspecting)//only display outline if isn't inspecting
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
        nameText.text = name;
        custoText.text = custo.ToString();
        descriptionText.text = cardDescription;
    }
    private void SetOpacity(float opacity)
    {
        Color color = originalOutlineColor;
        color.a = opacity;
        material.color = color;
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
}
