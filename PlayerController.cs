using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
/*COLOR PALLETTE
F94144
F3722C
F8961E
F9844A
F9C74F
90BE6D
43AA8B
4D908E
577590
277DA1
*/

public class PlayerController : MonoBehaviour
{
    /*
    Coisas pra adicionar:
    - Colocar e tirar cartas da mão

    Coisas pra consertar:
    - Múltiplas cartas podem ser colocadas no mesmo slot, ignorando os slotstates
    */
    private Camera mainCamera;
    private Rigidbody selectedRigidbody;
    private Collider selectedCollider;
    private Vector3 offset;
    private Plane dragPlane;
    private Vector3 targetPosition;
    public int playerHealth = 50;
    public bool isInspecting = false;
    [SerializeField]  private bool isReturning = false;
    [SerializeField] private bool isWalletOpen = false;
    [SerializeField] private ScoreKeeper scoreKeeperScript;
    private bool isLookingAtHands = false;
    private bool isLookingAtGame = false;
    private bool isPaused = false;
    private bool canCheckScroll = true;
    private bool canCheckUpScroll = true;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 inspectPosition = new Vector3(-0.66f, 8.52f, -17.85f);
    private Quaternion inspectRotation = Quaternion.Euler(30.47f, -180f, 0);
    private float mediumSmoothSpeed = 10f;
    private float fastSmoothSpeed = 20f;
    public bool canBuy = true;
    [SerializeField] private List<GameObject> cardsInHand;
    [SerializeField] private List<GameObject> cardDeck;
    [SerializeField] private List<GameObject> coins;
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private Image inspectOverlay;
    [SerializeField] private Color inspectColor;
    [SerializeField] private Color pauseColor;
    [SerializeField] private Color transparent;
    [SerializeField] private Color red;
    [SerializeField] private Color white;
    [SerializeField] private Color coinYellow;
    [SerializeField] private GameObject pauseObject;
    [SerializeField] private Image pauseOverlay;
    [SerializeField] private GameObject walletObject;
    [SerializeField] private Text healthText;
    [SerializeField] private Text coinsText;
    [SerializeField] private GameObject healthAndCoinsUIObject;
    [SerializeField] private GameObject returnFromWalletButton;
    [SerializeField] private GameObject enterWalletButton;
    [SerializeField] private GameObject fadeToBlack;
    public Dictionary<string, bool> slotStates = new Dictionary<string, bool>
    {
        { "SlotPersonagem", false },
        { "SlotDefesa", false },
        { "SlotMagia", false }
    };

    //FMOD
    private string CardPickUpSound = "event:/CardPickUp";
    private FMOD.Studio.EventInstance cardPickUpInstance;
    private string CardDropSound = "event:/CardDrop";
    private FMOD.Studio.EventInstance cardDropInstance;
    private string SelectSound = "event:/Select";
    private FMOD.Studio.EventInstance selectInstance;
    private string CancelSound = "event:/Cancel";
    private FMOD.Studio.EventInstance cancelInstance;
    private string SeeHandsSound = "event:/SeeHands";
    private FMOD.Studio.EventInstance seeHandsInstance;
    private string CloseHandsSound = "event:/CloseHands";
    private FMOD.Studio.EventInstance closeHandsInstance;
    private string SeeGameSound = "event:/SeeGame";
    private FMOD.Studio.EventInstance seeGameInstance;
    private string CloseGameSound = "event:/CloseGame";
    private FMOD.Studio.EventInstance closeGameInstance;
    private string FilledSlotSound = "event:/FilledSlot";
    private FMOD.Studio.EventInstance filledSlotInstance;
    private string BuyCardSound = "event:/BuyCard";
    private FMOD.Studio.EventInstance buyCardInstance;
    private string PauseSound = "event:/Pause";
    private FMOD.Studio.EventInstance pauseInstance;
    private string GetCoinsSound = "event:/GetCoins";
    private FMOD.Studio.EventInstance getCoinsInstance;
    void Start()
    {
        //camera
        mainCamera = Camera.main;

        //fmod
        cardPickUpInstance = RuntimeManager.CreateInstance(CardPickUpSound);
        cardDropInstance = RuntimeManager.CreateInstance(CardDropSound);
        selectInstance = RuntimeManager.CreateInstance(SelectSound);
        cancelInstance = RuntimeManager.CreateInstance(CancelSound);
        closeHandsInstance = RuntimeManager.CreateInstance(CloseHandsSound);
        seeHandsInstance = RuntimeManager.CreateInstance(SeeHandsSound);
        filledSlotInstance = RuntimeManager.CreateInstance(FilledSlotSound);
        buyCardInstance = RuntimeManager.CreateInstance(BuyCardSound);
        seeGameInstance = RuntimeManager.CreateInstance(SeeGameSound);
        closeGameInstance = RuntimeManager.CreateInstance(CloseGameSound);
        pauseInstance = RuntimeManager.CreateInstance(PauseSound);
        getCoinsInstance = RuntimeManager.CreateInstance(GetCoinsSound);
    }
    void Update()
    {
        if(scoreKeeperScript.playerACanPlay && !isPaused)
        {
            UI();
            if(scoreKeeperScript.playerACanMoveCards)
            {
                CardMovement();
            }
            if(scoreKeeperScript.playerACanPlay)
            {
                canBuy = scoreKeeperScript.playerACanBuy;
                CheckDeckClick();
                CheckCoinBagClick();
            }
            CardInspection();
            SeeHands();
            SeeGame();
        }
        playerHealth = scoreKeeperScript.playerAHealth;
        if(Input.GetKeyUp(KeyCode.Escape) && !isWalletOpen && !isLookingAtHands)
        {
            pauseInstance.start();
            isPaused = !isPaused;
        }
        PauseMethod();
        UpdateCoinCount();
        HUD();
        cameraAnimator.SetBool("LookToPlayerB", !scoreKeeperScript.AorB);
    }
    private bool IsPointerOverCard()
    {
        if(!isWalletOpen)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            return Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Card");
        }
        else
        {
            return false;
        }
    }
    private void UI()
    {
        Color targetColor = isInspecting ? inspectColor : transparent;
        inspectOverlay.color = Color.Lerp(inspectOverlay.color, targetColor, Time.deltaTime * fastSmoothSpeed);
    }
    private void PauseMethod()
    {
        pauseObject.SetActive(isPaused);
        Color targetColor = isPaused ? pauseColor : transparent;
        pauseOverlay.color = Color.Lerp(pauseOverlay.color, targetColor, Time.deltaTime * fastSmoothSpeed);
    }
    private void CardMovement()
    {
        if (Input.GetMouseButtonDown(0) && !isInspecting && !isReturning)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Card"))
            {
                bool canBeMoved = false;
                CardPersonagem cardPersonagem = hit.collider.GetComponent<CardPersonagem>();
                CardMagia cardMagia = hit.collider.GetComponent<CardMagia>();
                CardDefesa cardDefesa = hit.collider.GetComponent<CardDefesa>();
                if (cardPersonagem != null)
                {
                    canBeMoved = cardPersonagem.canBeMovedP;
                }
                else if (cardMagia != null)
                {
                    canBeMoved = cardMagia.canBeMovedM;
                }
                else if (cardDefesa != null)
                {
                    canBeMoved = cardDefesa.canBeMovedD;
                }

                if(canBeMoved)
                {
                    selectedRigidbody = hit.collider.GetComponent<Rigidbody>();
                    selectedCollider = hit.collider.GetComponent<Collider>();
                    UpdateSlotStateOnRemove();
                    selectedRigidbody.isKinematic = true; // Make the object kinematic while dragging
                    selectedCollider.enabled = false; // Disable the collider while dragging
                    dragPlane = new Plane(Vector3.up, hit.point);
                    offset = hit.point - selectedRigidbody.position;
                    targetPosition = selectedRigidbody.position;
                    targetPosition.y = 1.4f;
                    cardPickUpInstance.start();
                }
                else
                {
                    Debug.LogWarning("Card can't be moved");
                }
            }
        }
        if (Input.GetMouseButton(0) && selectedRigidbody != null && !isInspecting && !isReturning)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;
            if (dragPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                targetPosition = hitPoint - offset;
                targetPosition.y = 1.4f;
                selectedRigidbody.MovePosition(Vector3.Lerp(selectedRigidbody.position, targetPosition, mediumSmoothSpeed * Time.deltaTime));
            }
        }
        if (Input.GetMouseButtonUp(0) && selectedRigidbody != null && !isInspecting && !isReturning)
        {
            SnapToSlot();
            if(selectedRigidbody.position.z < -20f)
            {
                MoveCardToHand(selectedRigidbody.gameObject);
            }
            selectedRigidbody.isKinematic = false;
            selectedRigidbody.velocity = Vector3.zero;
            selectedRigidbody.angularVelocity = Vector3.zero;
            selectedCollider.enabled = true;
            selectedRigidbody = null;
            selectedCollider = null;
            cardDropInstance.start();
        }
    }
    private void CardInspection()
    {
        if (Input.GetMouseButtonDown(1) && !isInspecting && !isReturning)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Card"))
            {
                selectedRigidbody = hit.collider.GetComponent<Rigidbody>();
                selectedCollider = hit.collider.GetComponent<Collider>();
                originalPosition = selectedRigidbody.position;
                originalRotation = selectedRigidbody.rotation;
                selectedRigidbody.isKinematic = true;
                isInspecting = true;
                isLookingAtGame = false;
                isLookingAtHands = false;
                selectInstance.start();
            }
        }
        if (isInspecting && selectedRigidbody != null)
        {
            selectedRigidbody.position = Vector3.Lerp(selectedRigidbody.position, inspectPosition, mediumSmoothSpeed * Time.deltaTime);
            selectedRigidbody.rotation = Quaternion.Lerp(selectedRigidbody.rotation, inspectRotation, mediumSmoothSpeed * Time.deltaTime);
        }
        if (isReturning && selectedRigidbody != null)
        {
            selectedRigidbody.position = Vector3.Lerp(selectedRigidbody.position, originalPosition, fastSmoothSpeed * Time.deltaTime);
            selectedRigidbody.rotation = Quaternion.Lerp(selectedRigidbody.rotation, originalRotation, fastSmoothSpeed * Time.deltaTime);
            if (Vector3.Distance(selectedRigidbody.position, originalPosition) < 0.01f && Quaternion.Angle(selectedRigidbody.rotation, originalRotation) < 0.5f)
            {
                selectedRigidbody.position = originalPosition;
                selectedRigidbody.rotation = originalRotation;
                selectedRigidbody.isKinematic = false;
                selectedCollider.enabled = true;
                isReturning = false;
                selectedRigidbody = null;
                selectedCollider = null;
            }
        }
        if ((Input.GetKeyDown(KeyCode.Escape) || (Input.GetMouseButtonDown(0))) && isInspecting)
        {
            cancelInstance.start();
            isInspecting = false;
            isReturning = true;
        }
    }
    private void SnapToSlot()
    {
        Collider[] hitColliders = Physics.OverlapBox(selectedRigidbody.position, selectedRigidbody.transform.localScale / 2);
        foreach (var hitCollider in hitColliders)
        {
            string slotTag = hitCollider.tag;
            string cardTag = selectedRigidbody.tag;
            Debug.Log(slotTag);

            if (slotStates.ContainsKey(slotTag) && !slotStates[slotTag] && slotTag.Replace("Slot", "") == cardTag)
            {
                Vector3 slotPosition = hitCollider.transform.position;
                selectedRigidbody.position = new Vector3(slotPosition.x, selectedRigidbody.position.y, slotPosition.z);
                slotStates[slotTag] = true;
                filledSlotInstance.start();
                SoundEffects soundEffects = FindObjectOfType<SoundEffects>();
                soundEffects.PlayCardSound(selectedRigidbody.name.Replace("(Clone)", "").Trim() + "Activate");

                if(cardTag == "Personagem")
                {
                    scoreKeeperScript.playerAHasCharacter = true;
                    CardPersonagem cardPScript = selectedRigidbody.GetComponent<CardPersonagem>();
                    cardPScript.canBeMovedP = false;
                    cardPScript.cardIsActive = true;
                }
                if(cardTag == "Defesa")
                {
                    scoreKeeperScript.playerAHasDefense = true;
                    CardDefesa cardDScript = selectedRigidbody.GetComponent<CardDefesa>();
                    cardDScript.canBeMovedD = false;
                }
                if(cardTag == "Magia")
                {
                    CardMagia cardMScript = selectedRigidbody.GetComponent<CardMagia>();
                    cardMScript.canBeMovedM = false;
                }
                break;
            }
        }
    }
    private void UpdateCoinCount()
    {
        int numberOfChildren = walletObject.transform.childCount;
        scoreKeeperScript.playerACoins = numberOfChildren;
    }
    public void RemoveCoins(int cost)
    {
        int numberOfChildren = walletObject.transform.childCount;
        if (numberOfChildren >= cost)
        {
            for (int i = 0; i < cost; i++)
            {
                Transform child = walletObject.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            UpdateCoinCount();
        }
    }
    public void ToggleWallet()
    {
        isWalletOpen = !isWalletOpen;
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
    }
    private void UpdateSlotStateOnRemove()
    {
        if (selectedRigidbody == null)
        {
            return;
        }
        Collider[] hitColliders = Physics.OverlapBox(selectedRigidbody.position, selectedRigidbody.transform.localScale / 2);
        foreach (var hitCollider in hitColliders)
        {
            string slotTag = hitCollider.tag;
            string cardTag = selectedRigidbody.tag;

            if (slotStates.ContainsKey(slotTag) && slotTag.Replace("Slot", "") == cardTag)
            {
                slotStates[slotTag] = false;
                break;
            }
        }
    }
    private void SeeHands()
    {
        if(!isWalletOpen && !isInspecting && !isLookingAtGame && canCheckScroll)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0 && isLookingAtHands)
            {
                seeHandsInstance.start();
                isLookingAtHands = false;
                StartCoroutine(DelayScrollCheck());
                StartCoroutine(DelayUpScrollCheck());
            }
            else if (scroll < 0 && !isLookingAtHands && !isLookingAtGame)
            {
                closeHandsInstance.start();
                isLookingAtHands = true;
                StartCoroutine(DelayScrollCheck());
                StartCoroutine(DelayUpScrollCheck());
            }
        }
        cameraAnimator.SetBool("LookAtHands", isLookingAtHands);
    }
    private void SeeGame()
    {
        if(!isWalletOpen && !isInspecting && !isLookingAtHands && canCheckUpScroll)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                isLookingAtGame = true;
                seeGameInstance.start();
                StartCoroutine(DelayUpScrollCheck());
                StartCoroutine(DelayScrollCheck());
            }
            else if (scroll < 0)
            {
                isLookingAtGame = false;
                closeGameInstance.start();
                StartCoroutine(DelayUpScrollCheck());
                StartCoroutine(DelayScrollCheck());
            }
        }
        cameraAnimator.SetBool("LookToGame", isLookingAtGame);
    }
    private void HUD()
    {
        //only show health and coin HUD if wallet is closed
        healthAndCoinsUIObject.SetActive(!isWalletOpen);
        healthText.text = "Vida: " + playerHealth.ToString();
        coinsText.text = "Moedas: " + scoreKeeperScript.playerACoins.ToString();
        returnFromWalletButton.SetActive(isWalletOpen);
        enterWalletButton.SetActive(!isWalletOpen);
        fadeToBlack.SetActive(!isInspecting);

        if(isWalletOpen && Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            cameraAnimator.Play("ReturnFromWallet");
            isLookingAtGame = false;
            isLookingAtHands = false;
            isWalletOpen = false;
        }
        if(isLookingAtHands && Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            isLookingAtHands = false;
        }
    }
    private void CheckDeckClick()
    {
        if (Input.GetMouseButtonDown(0) && canBuy)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Deck"))
            {
                buyCardInstance.start();
                InstantiateRandomCard();
                scoreKeeperScript.turnStep = 1;
                canBuy = false;
            }
        }
    }
    private void CheckCoinBagClick()
    {
        if(Input.GetMouseButtonDown(0) && canBuy)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("CoinBag"))
            {
                getCoinsInstance.start();
                InstantiateCoin();//instantiate two coins
                InstantiateCoin();
                scoreKeeperScript.turnStep = 1;
                canBuy = false;
            }
        }
    }
    private void InstantiateRandomCard()
    {
        if (cardDeck.Count > 0 && canBuy)
        {
            int randomIndex = Random.Range(0, cardDeck.Count);
            GameObject newCard = Instantiate(cardDeck[randomIndex], cardSpawnPoint.position, Quaternion.Euler(0, -180, 0));

            cardDeck.RemoveAt(randomIndex);

            CardPersonagem cardPersonagem = newCard.GetComponent<CardPersonagem>();
            CardMagia cardMagia = newCard.GetComponent<CardMagia>();
            CardDefesa cardDefesa = newCard.GetComponent<CardDefesa>();

            if (cardPersonagem != null)
            {
                cardPersonagem.canBeMovedP = true;
            }
            else if (cardMagia != null)
            {
                cardMagia.canBeMovedM = true;
            }
            else if (cardDefesa != null)
            {
                cardDefesa.canBeMovedD = true;
            }
        }
    }
    private void InstantiateCoin()
    {
        int randomIndex = Random.Range(0, coins.Count);
        GameObject newCoin = Instantiate(coins[randomIndex], coinSpawnPoint.position, coinSpawnPoint.rotation);
        newCoin.transform.SetParent(walletObject.transform);
    }
    private void MoveCardToHand(GameObject card)
    {
        cardsInHand.Add(card);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    private IEnumerator DelayScrollCheck()
    {
        canCheckScroll = false;
        yield return new WaitForSeconds(0.2f);
        while (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            yield return null;
        }
        canCheckScroll = true;
    }
    private IEnumerator DelayUpScrollCheck()
    {
        canCheckUpScroll = false;
        yield return new WaitForSeconds(0.25f);
        while (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            yield return null;
        }
        canCheckUpScroll = true;
    }
    public IEnumerator NotEnoughCoinsAnim()
    {
        coinsText.color = Color.Lerp(coinsText.color, red, Time.deltaTime * fastSmoothSpeed);
        yield return new WaitForSeconds(0.1f);
        coinsText.color = Color.Lerp(coinsText.color, white, Time.deltaTime * mediumSmoothSpeed);
        coinsText.color = white;
        yield return null;
    }
    public IEnumerator SpentCoinsAnim()
    {
        coinsText.color = Color.Lerp(coinsText.color, coinYellow, Time.deltaTime * mediumSmoothSpeed);
        yield return new WaitForSeconds(0.1f);
        coinsText.color = Color.Lerp(coinsText.color, white, Time.deltaTime * mediumSmoothSpeed);
        coinsText.color = white;
        yield return null;
    }
}