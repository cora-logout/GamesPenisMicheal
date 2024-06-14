using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class ScoreKeeper : MonoBehaviour
{
    /*
        Coisas pra fazer:
        Adicionar lógica de efeitos (veneno, sangramento, etc)
        Talvez usar um switch case com duas opções, string e int pro nome e força
    */
    private static ScoreKeeper _instance;
    public int playerAHealth;
    public int playerBHealth;
    public int characterAHealth;
    public int characterBHealth;
    public int defenseAHealth;
    public int defenseBHealth;
    public int turnStep = 0;
    public int currentTurn = -1;
    public int playerACoins = 0;
    public int playerBCoins = 0;
    public bool playerAHasCharacter = false;
    public bool playerBHasCharacter = false;
    public bool playerAHasDefense = false;
    public bool playerBHasDefense = false;
    public bool playerACanPlay = true;
    public bool playerBCanPlay = false;
    public bool playerACanMoveCards = false;
    public bool playerBCanMoveCards = false;
    public bool playerACanBuy = false;
    public bool playerBCanBuy = false;
    public bool playerAPreTurnOver = false;
    public bool playerBPreTurnOver = false;
    public bool playerACanAttack = false;
    public bool playerBCanAttack = false;
    private bool AorB;
    public PlayerController playerAController;
    /*
        turnStep 0 = Can buy two coins or one card
        turnStep 1 = Can move, place cards and attack
        turnStep 2 = End turn, switch turns
    */
    public static ScoreKeeper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreKeeper>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ScoreKeeper");
                    _instance = singletonObject.AddComponent<ScoreKeeper>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    private void Awake()
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
    }
    private void Start()
    {
        playerAHealth = 50;
        playerBHealth = 50;
    }
    void Update()
    {
        if(playerACanPlay)
        {
            if(playerAPreTurnOver)
            {
                PlayerATurn();
            }
            else if(!playerAPreTurnOver)
            {
                PlayerAPreTurn();
            }
        }
        if(playerBCanPlay)
        {
            if(playerBPreTurnOver)
            {
                PlayerBTurn();
            }
            else if(!playerBPreTurnOver)
            {
                PlayerBPreTurn();
            }
        }
        if(turnStep == 2)
        {
            SwitchTurns();
        }
        DamageLogic();
    }

    void PlayerATurn()
    {
        playerACanBuy = (turnStep == 0);
        playerACanMoveCards = (turnStep == 1);
        playerACanAttack = (turnStep == 1);
    }
    void PlayerBTurn()
    {
        playerBCanBuy = (turnStep == 0);
        playerBCanMoveCards = (turnStep == 1);
        playerBCanAttack = (turnStep == 1);
    }
    void PlayerAPreTurn()
    {
        if(turnStep == 0)
        {
            turnStep = 1;
        }
        playerACanMoveCards = (turnStep == 1);
        if(playerAHasCharacter && playerAHasDefense)
        {
            playerACanAttack = true;
            playerAPreTurnOver = true;
            turnStep = 2;
        }
    }
    void PlayerBPreTurn()
    {
        if(turnStep == 0)
        {
            turnStep = 1;
        }
        playerBCanMoveCards = (turnStep == 1);
        if(playerBHasCharacter && playerBHasDefense)
        {
            playerBCanAttack = true;
            playerBPreTurnOver = true;
            turnStep = 2;
        }
    }

    public void SwitchTurns()
    {
        playerACanPlay = !playerACanPlay;
        playerBCanPlay = !playerBCanPlay;
        currentTurn = currentTurn + 1;
        turnStep = 0;
    }
    private void DamageLogic()
    {
        AorB = (playerACanAttack && playerACanPlay);
    }
    public void DoDamage(int damage)
    {
        if(AorB && playerACanAttack)//A is doing damage to B
        {
            if(playerBHasDefense && defenseBHealth > 0)
            {
                defenseBHealth = defenseBHealth - damage;
                return;
            }
            else if(playerBHasCharacter && characterBHealth > 0)
            {
                characterBHealth = characterBHealth - damage;
                return;
            }
            else
            {
                playerBHealth = playerBHealth - damage;
                return;
            }
        }
        if(!AorB && playerBCanAttack)//B is doing damage to A
        {
            if(playerAHasDefense && defenseAHealth > 0)
            {
                defenseAHealth = defenseAHealth - damage;
                return;
            }
            else if(playerAHasCharacter && characterAHealth > 0)
            {
                characterAHealth = characterAHealth - damage;
                return;
            }
            else
            {
                playerAHealth = playerAHealth - damage;
                return;
            }
        }
    }
}
