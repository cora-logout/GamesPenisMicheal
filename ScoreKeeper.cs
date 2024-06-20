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
    public bool AorB;
    [SerializeField] private bool canRain = false;
    [SerializeField] private bool canSnow = false;
    [SerializeField] private bool isRaining = false;
    [SerializeField] private bool isSnowing = false;
    public int rainChance = 9;
    public int snowChance = 9;
    private int defaultRainChance = 9;
    private int defaultSnowChance = 9;
    public int rainMaxParticles = 300;
    public int snowMaxParticles = 1000;
    private bool hasRained = false;
    private bool hasSnowed = false;
    [SerializeField] private ParticleSystem rainPS;
    [SerializeField] private ParticleSystem snowPS;
    [SerializeField] private ParticleSystem dustPS;
    [SerializeField] private Image snowOverlay;
    [SerializeField] private Color transparent;
    [SerializeField] private Color semiOpaque;
    [SerializeField] private Color opaque;
    [SerializeField] private GameObject dudeObject;
    private Vector3 dudeDefaultPosition = new Vector3(-0.5f, 5.91f, -16.91f);
    private Vector3 dudePosition = new Vector3(0, 8.86f, -18.25f);
    private Quaternion dudeDefaultRotation = Quaternion.Euler(-25.299f, 0, 0);
    private Quaternion dudeRotation = Quaternion.Euler(48.864f, 0, 0);
    //FMOD
    private string RainSound = "event:/Ambience/Rain";
    private FMOD.Studio.EventInstance rainInstance;
    private string SnowSound = "event:/Ambience/Snow";
    private FMOD.Studio.EventInstance snowInstance;
    private string Music = "event:/Music/PlaceHolder";
    private FMOD.Studio.EventInstance musicInstance;
    private string ShowDudeSound = "event:/ShowDude";
    private FMOD.Studio.EventInstance showDudeInstance;
    private string HideDudeSound = "event:/HideDude";
    private FMOD.Studio.EventInstance hideDudeInstance;
    public PlayerController playerAController;
    /*Turn logic
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
        musicInstance = RuntimeManager.CreateInstance(Music);
        musicInstance.start();
        playerAHealth = 50;
        playerBHealth = 50;
        rainInstance = RuntimeManager.CreateInstance(RainSound);
        snowInstance = RuntimeManager.CreateInstance(SnowSound);
        showDudeInstance = RuntimeManager.CreateInstance(ShowDudeSound);
        hideDudeInstance = RuntimeManager.CreateInstance(HideDudeSound);
        dudeObject.SetActive(false);
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
        Weather();
    }
    void PlayerATurn()
    {
        playerACanBuy = (turnStep == 0);
        playerBCanBuy = false;
        playerACanMoveCards = (turnStep == 1);
        playerBCanMoveCards = false;
        playerACanAttack = (turnStep == 1);
        playerBCanAttack = false;
    }
    void PlayerBTurn()
    {
        playerBCanBuy = (turnStep == 0);
        playerACanBuy = false;
        playerBCanMoveCards = (turnStep == 1);
        playerACanMoveCards = false;
        playerBCanAttack = (turnStep == 1);
        playerACanAttack = false;
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
        if(canRain)
        {
            RollRain();
        }
        if(canSnow)
        {
            RollSnow();
        }
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
    private void Weather()
    {
        Rain();
        Snow();
    }
    private void Rain()
    {
        if(currentTurn >= 4)
        {
            canRain = true;
        }
        rainInstance.setPaused(!isRaining);

        var em = rainPS.emission;
        em.enabled = isRaining;
        var rainPSMain = rainPS.main;
        int rainTargetParticles = isRaining ? rainMaxParticles : 0;
        rainPSMain.maxParticles = (int)Mathf.Lerp(rainPSMain.maxParticles, rainTargetParticles, Time.deltaTime * 2.5f);
        if(isRaining)
        {  
            //lower fire damage, increase water damage
        }
    }
    private void RollRain()
    {
        //"roll dice" to decide if it's raining or not
        int randomIndex = Random.Range(0, rainChance);
        if(randomIndex == rainChance - 1 && !isSnowing)//only rain if it isn't snowing
        {
            rainChance++;
            Debug.Log("Rain chance is now: " + rainChance);
            rainInstance.start();
            isRaining = true;
            hasRained = true;
        }
        else 
        {
            if(hasRained)
            {
                defaultRainChance = 11;
            }
            rainChance = defaultRainChance;
            isRaining = false;
        }
    }
    private void Snow()
    {
        if(currentTurn >= 4)
        {
            canSnow = true;
        }
        snowInstance.setPaused(!isSnowing);
        Color targetColor = isSnowing ? semiOpaque : transparent;
        snowOverlay.color = Color.Lerp(snowOverlay.color, targetColor, Time.deltaTime * 1f);
        
        var em = snowPS.emission;
        em.enabled = isSnowing;
        var snowPSMain = snowPS.main;
        int snowTargetParticles = isSnowing ? snowMaxParticles : 0;
        snowPSMain.maxParticles = (int)Mathf.Lerp(snowPSMain.maxParticles, snowTargetParticles, Time.deltaTime * 5f);
        if(isSnowing)
        {
            //lower water damage, increase ice damage
        }
    }
    private void RollSnow()
    {
        int randomIndex = Random.Range(0, snowChance);
        if(randomIndex == snowChance - 1 && !isRaining)//only snow if it isn't raining
        {
            snowChance++;
            Debug.Log("Snow chance is now: " + snowChance);
            snowInstance.start();
            isSnowing = true;
            hasRained = true;

            if(isSnowing)//if already snowing, increase chance to stop
            {
                snowChance++;
            }
        }
        else
        {
            if(hasSnowed)
            {
                defaultSnowChance = 11;
            }
            snowChance = defaultSnowChance;
            isSnowing = false;
        }
    }
    public void DudePopUp()
    {
        StartCoroutine(ShowDude());
    }
    private IEnumerator ShowDude()
    {
        dudeObject.SetActive(true);
        float duration = 0.1f;
        float waitTime = 1f;
        float elapsedTime = 0f;
        Vector3 startPosition = dudeObject.transform.position;
        Quaternion startRotation = dudeObject.transform.rotation;

        while (elapsedTime < duration)
        {
            dudeObject.transform.position = Vector3.Lerp(startPosition, dudePosition, elapsedTime / duration);
            dudeObject.transform.rotation = Quaternion.Lerp(startRotation, dudeRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dudeObject.transform.position = dudePosition;
        dudeObject.transform.rotation = dudeRotation;
        showDudeInstance.start();

        yield return new WaitForSeconds(waitTime);
        elapsedTime = 0f;
        startPosition = dudeObject.transform.position;
        startRotation = dudeObject.transform.rotation;

        while (elapsedTime < duration)
        {
            dudeObject.transform.position = Vector3.Lerp(startPosition, dudeDefaultPosition, elapsedTime / duration);
            dudeObject.transform.rotation = Quaternion.Lerp(startRotation, dudeDefaultRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dudeObject.transform.position = dudeDefaultPosition;
        dudeObject.transform.rotation = dudeDefaultRotation;
        hideDudeInstance.start();
        dudeObject.SetActive(false);
        yield return null;
    }
}