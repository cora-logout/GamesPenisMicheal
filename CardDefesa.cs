using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD;
using FMOD.Studio;

public class CardDefesa : MonoBehaviour
{
    [SerializeField] private int health = 20;
    [SerializeField] private Text healthText;
    [SerializeField] private new string name = "Default";
    [SerializeField] private Text nameText;
    [SerializeField] private string description = "Default";
    [SerializeField] private Text descriptionText;
    public bool canBeMovedD = false;
    void Awake()
    {
        nameText.text = name;
        healthText.text = health.ToString();
        descriptionText.text = description;
    }
    void Update()
    {
        UpdateTexts();
    }
    private void UpdateTexts()
    {
        nameText.text = name;
        healthText.text = health.ToString();
    }
}
