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
    private int currentTurn = -1;
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
    private int dustMaxParticles = 50;
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
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private Animator peAnimator;
    private Vector3 dudeDefaultPosition = new Vector3(-0.5f, 5.91f, -16.91f);
    private Vector3 dudePosition = new Vector3(0, 8.86f, -18.25f);
    private Quaternion dudeDefaultRotation = Quaternion.Euler(-25.299f, 0, 0);
    private Quaternion dudeRotation = Quaternion.Euler(48.864f, 0, 0);
    public enum Song
    {
        TFYAB,
        YPITS
    }
    public enum Room
    {
        WoodRoom,
        TechRoom,
        WeirdRoom
    }
    [SerializeField] private Song songSelector;
    [SerializeField] private Room roomSelector;
    //FMOD
    private string RainSound = "event:/Ambience/Rain";
    private FMOD.Studio.EventInstance rainInstance;
    private string SnowSound = "event:/Ambience/Snow";
    private FMOD.Studio.EventInstance snowInstance;
    private string Music = "Song path";
    private FMOD.Studio.EventInstance musicInstance;
    private string ShowDudeSound = "event:/ShowDude";
    private FMOD.Studio.EventInstance showDudeInstance;
    private string HideDudeSound = "event:/HideDude";
    private FMOD.Studio.EventInstance hideDudeInstance;
    private string KickTableSound = "event:/KickTable";
    private FMOD.Studio.EventInstance kickTableInstance;
    private float basslineValue = 0;
    private float funkyBasslineValue = 0;
    private float shakerValue = 0;
    private float YPITSFireLeadValue = 0;
    private float YPITSSnareValue = 0;
    private float YPITSVinylHornsValue = 0;
    [SerializeField] private GameObject woodRoom;
    [SerializeField] private GameObject techRoom;
    [SerializeField] private GameObject weirdRoom;
    private Vector3 roomSpawnPos = new Vector3(83.1f, 65.3914f, 128.8959f);

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
        switch(roomSelector)
        {
            case Room.WoodRoom:
                Instantiate(woodRoom, roomSpawnPos, transform.rotation);
                break;
            case Room.TechRoom:
                Instantiate(techRoom, roomSpawnPos, transform.rotation);
                break;
            case Room.WeirdRoom:
                Instantiate(weirdRoom, roomSpawnPos, transform.rotation);
                break;
            default: 
                Debug.LogError("Room not selected");
                break;
        }
    }
    private void Start()
    {
        AorB = true;
        playerAHealth = 50;
        playerBHealth = 50;
        rainInstance = RuntimeManager.CreateInstance(RainSound);
        snowInstance = RuntimeManager.CreateInstance(SnowSound);
        showDudeInstance = RuntimeManager.CreateInstance(ShowDudeSound);
        hideDudeInstance = RuntimeManager.CreateInstance(HideDudeSound);
        kickTableInstance = RuntimeManager.CreateInstance(KickTableSound);
        dudeObject.SetActive(false);
        switch(songSelector)
        {
            case Song.TFYAB:
                Music = "event:/Music/TimeForYetAnotherBattle";
                musicInstance = RuntimeManager.CreateInstance(Music);
                musicInstance.setParameterByName("TFYABBassline", 1);
                musicInstance.setParameterByName("TFYABEpiano", 1);
                musicInstance.setParameterByName("TFYABKick", 1);
                break;
            case Song.YPITS:
                Music = "event:/Music/YourPlaceInTheSaloon";
                musicInstance = RuntimeManager.CreateInstance(Music);
                musicInstance.setParameterByName("YPITSVinylHorns", 0);
                musicInstance.setParameterByName("YPITSClick", 1);
                musicInstance.setParameterByName("YPITSVinylCrash", 1);
                musicInstance.setParameterByName("YPITSWorkPerc", 1);
                break;
            default:
                Debug.LogWarning("Unknown song selected");
                break;
        }
        musicInstance.start();
    }
    void Update()
    {
        if(playerACanPlay)
        {
            if(playerAPreTurnOver)
            {
                PlayerATurn();
            }
            else
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
            else
            {
                PlayerBPreTurn();
            }
        }
        if(turnStep == 2)
        {
            SwitchTurns();
        }
        Weather();
        MusicControl();
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
    private void MusicControl()
    {
        switch(songSelector)
        {
            case Song.TFYAB:
                if (currentTurn > 0)
                {
                    if (turnStep == 0)//Bassline ON, FunkyBasline OFF, Shaker OFF
                    {
                        StartCoroutine(LerpParameter("TFYABBassline", basslineValue, 1, 2));
                        StartCoroutine(LerpParameter("TFYABFunkyBassline", funkyBasslineValue, 0, 2));
                        StartCoroutine(LerpParameter("TFYABShaker", shakerValue, 0, 2));
                    }
                    else if (turnStep == 1)//Bassline OFF, FunkyBasline ON, Shaker ON
                    {
                        StartCoroutine(LerpParameter("TFYABBassline", basslineValue, 0, 2));
                        StartCoroutine(LerpParameter("TFYABFunkyBassline", funkyBasslineValue, 1, 2));
                        StartCoroutine(LerpParameter("TFYABShaker", shakerValue, 1, 2));
                    }
                }
                break;
            case Song.YPITS:
                if(currentTurn > 0)
                {
                    if(turnStep == 0)//VinylHorns ON, FireLead OFF, Snare OFF
                    {
                        StartCoroutine(LerpParameter("YPITSVinylHorns", YPITSVinylHornsValue, 1, 2));
                        StartCoroutine(LerpParameter("YPITSFireLead", YPITSFireLeadValue, 0, 2));
                        StartCoroutine(LerpParameter("YPITSSnare", YPITSSnareValue, 0, 2));
                    }
                    else if(turnStep == 1)//FireLead ON, Snare ON
                    {
                        StartCoroutine(LerpParameter("YPITSFireLead", YPITSFireLeadValue, 1, 2));
                        StartCoroutine(LerpParameter("YPITSSnare", YPITSSnareValue, 1, 2));
                    }
                }
                break;
            default:
                Debug.LogWarning("Unknown song selected");
                break;
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
        AorB = !AorB;
        turnStep = 0;
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
        snowOverlay.color = Color.Lerp(snowOverlay.color, targetColor, Time.deltaTime * 1);
        
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
    public void KickTable()
    {
        StartCoroutine(KickTableAnim());
    }
    public void StealRandomCard(bool AorB)
    {
        if(AorB)//A is stealing from B
        {
            Debug.Log("Steal random card from player B");
        }
        else//B is stealing from A
        {
            Debug.Log("Steal random card from player A");
        }
    }
    private IEnumerator ShowDude()
    {
        dudeObject.SetActive(true);
        float duration = 0.1f;
        float waitTime = 1;
        float elapsedTime = 0;
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
        elapsedTime = 0;
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
    private IEnumerator KickTableAnim()
    {
        //move camera and disable dust
        var dustPSMain = dustPS.main;
        dustPSMain.maxParticles = (int)Mathf.Lerp(dustPSMain.maxParticles, 0, Time.deltaTime * 25f);
        cameraAnimator.SetBool("KickTable", true);
        
        yield return new WaitForSeconds(0.75f);
        //start kick
        peAnimator.SetBool("Kick", true);

        yield return new WaitForSeconds(0.4f);
        //sfx
        kickTableInstance.start();

        yield return new WaitForSeconds(0.5f);
        //finish kick
        peAnimator.SetBool("Kick", false);

        yield return new WaitForSeconds(0.15f);
        //return camera and dust
        dustPSMain.maxParticles = (int)Mathf.Lerp(0, dustMaxParticles, Time.deltaTime * 25f);
        cameraAnimator.SetBool("KickTable", false);
    }
    private IEnumerator LerpParameter(string parameterName, float fromValue, float toValue, float speed)
    {
        float elapsedTime = 0;
        while (elapsedTime < 50f)
        {
            elapsedTime += Time.deltaTime * speed;
            float newValue = Mathf.Lerp(fromValue, toValue, elapsedTime / 50f);
            musicInstance.setParameterByName(parameterName, newValue);
            yield return null;
        }
        musicInstance.setParameterByName(parameterName, toValue);
    }
}