using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public string cardName;
    public int skillNumber;
    public bool isActive = true;//controls if skill can be played or not
    public int coolDown = 1;//reference coolDown for this skill
    public int currentCoolDown = 0;//currentCoolDown == 0 means skill can be played
    private Color gray;
    private Color black;
    [SerializeField] private Text skillText;
    private ScoreKeeper scoreKeeper;

    private void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
        black = new Color(0, 0, 0, 1);
        gray = new Color(0.5f, 0.5f, 0.5f, 1);
    }

    private void Update()
    {
        if(scoreKeeper.turnStep == 2)
        {
            Cooldown();
        }
    }
    
    public void Cooldown()
    {
        if(currentCoolDown > 0)
        {
            isActive = false;
            currentCoolDown = currentCoolDown -1;
            skillText.color = gray;
            skillText.fontStyle = FontStyle.Italic;
        }
        else
        {
            isActive = true;
            skillText.color = black;
            skillText.fontStyle = FontStyle.Normal;
        }
    }

    public void SetCooldown()
    {
        currentCoolDown = coolDown;
    }
}
