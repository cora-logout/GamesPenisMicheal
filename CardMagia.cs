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
    public bool canBeMovedM = false;
    void Awake()
    {
        nameText.text = name;
        custoText.text = custo.ToString();
        descriptionText.text = cardDescription;
    }
    void Update()
    {
        nameText.text = name;
        custoText.text = custo.ToString();
        descriptionText.text = cardDescription;
    }
}
