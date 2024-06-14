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
            //Activate
            { "Billy Feijão Do MalActivate", "event:/Characters/Activate/Billy"},
            { "Bufos RegularisActivate", "event:/Characters/Activate/ActivateFrog" },
            { "CaixaDeSomActivate", "event:/Characters/Activate/ActivateCaixaDeSom" },
            { "FantasmaActivate", "event:/Characters/Activate/ActivateFantasma"},
            { "MeloActivate", "event:/Characters/Activate/ActivateMelo" },
            { "TorreDoMagoActivate", "event:/Characters/Activate/ActivateTorreDoMago" },
            { "PoteDePiclesActivate", "event:/Characters/Activate/ActivatePoteDePicles" },
            { "CupcakeActivate", "event:/Characters/Activate/ActivateCupcake" },
            //Attack
            { "Billy Feijão do MalAttack", "event:/Characters/Attack/Billy"},
            { "Bufos RegularisAttack", "event:/Characters/Attack/AttackFrog" },
            { "GatoAttack", "event:/Characters/Attack/AttackCat" }
            //Death
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
