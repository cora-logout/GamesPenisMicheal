using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundEffects : MonoBehaviour
{
    private Dictionary<string, string> cardSoundMap;
    void Awake()
    {
        cardSoundMap = new Dictionary<string, string>
        {
        //Activate DEF
            { "CaixaDeSomActivate", "event:/Defenses/Activate/CaixaDeSom" },
            { "CasacoDeFrioActivate", "event:/Defenses/Activate/CasacoDeFrio"},
        //Activate MAG
            { "CupcakeActivate", "event:/Magic/Cupcake" },
            { "TorreDoMagoActivate", "event:/Magic/TorreDoMago" },
            { "PoteDePiclesActivate", "event:/Magic/PoteDePicless" },
        //Activate CHAR
            { "Billy Feijão Do MalActivate", "event:/Characters/Activate/Billy"},
            { "Bufos RegularisActivate", "event:/Characters/Activate/BufosRegularis" },
            { "Doutor AfanasfávioActivate", "event:/Characters/Activate/DoutorAfanasfávio"},
            { "FantasmaActivate", "event:/Characters/Activate/Fantasma"},
            { "MeloActivate", "event:/Characters/Activate/Melo" },
        //Attack CHAR
            { "Billy Feijão do MalAttack", "event:/Characters/Attack/Billy"},
            { "Bufos RegularisAttack", "event:/Characters/Attack/BufosRegularis" },
            { "Doutor AfanasfávioAttack", "event:/Characters/Attack/DoutorAfanasfávio"}
        //Death CHAR
        //Death DEF
        };
    }

    public void PlayCardSound(string cardName)
    {
        if (cardSoundMap.ContainsKey(cardName))
        {
            string soundEventPath = cardSoundMap[cardName];
            EventInstance soundInstance = RuntimeManager.CreateInstance(soundEventPath);
            soundInstance.start();
            soundInstance.release();
        }
        else
        {
            Debug.LogWarning("No sound mapped for card: " + cardName);
        }
    }
}
