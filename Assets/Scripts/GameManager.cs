using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AudioSource BGMPlayer;

    void Awake()
    {
        //initialize sfx manager
        AudioManager.Initialize(BGMPlayer);
    }
}
