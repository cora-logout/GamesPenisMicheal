using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public string cardName;
    public int skillNumber;
    public bool isActive = true;//controls if skill can be played or not
    public int coolDown = 1;//reference coolDown for this skill
    public int currentCoolDown = 0;//currentCoolDown == 0 means skill can be played
    [SerializeField] private Color gray;
    [SerializeField] private Color black;
    [SerializeField] private Text skillText;
    private ScoreKeeper scoreKeeper;

    private void Start()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>();
    }
    
    public void Cooldown()//call this every turn
    {
        if(currentCoolDown == 0)
        {
            Debug.Log("Skill was played, setting currentCoolDown to: " + coolDown);
            currentCoolDown = coolDown;
        }

        if(currentCoolDown > 0)
        {
            isActive = false;
            currentCoolDown = currentCoolDown -1;
            skillText.color = gray;
        }
        else
        {
            isActive = true;
            skillText.color = black;
            Debug.Log("Cooldown over, skill is active again");
        }
    }
}
