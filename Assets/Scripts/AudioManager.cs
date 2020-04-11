using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class AudioManager
{
    const string PATH_TO_SFX_FOLDER = "SFX";

    const string PATH_TO_BGM_FOLDER = "BGM";

    const string PATH_TO_AUDIO_PLAYER_PREFAB = "Prefabs/AudioPlayer";

    private static GameObject m_audioPLayerPrefab;

    private static Dictionary<int, AudioClip> m_sfxs;

    private static bool m_initialed = false;

    private static AudioSource m_bgmPLayer;

    public static void Initialize(AudioSource bgmPLayer)
    {
        if (m_initialed)
            return;

        m_sfxs = new Dictionary<int, AudioClip>();

        AudioClip[] tempSfxs = Resources.LoadAll<AudioClip>(PATH_TO_SFX_FOLDER);

        foreach (AudioClip audioClip in tempSfxs)
            m_sfxs.Add(int.Parse(audioClip.name), audioClip);

        m_audioPLayerPrefab = Resources.Load<GameObject>(PATH_TO_AUDIO_PLAYER_PREFAB);

        m_bgmPLayer = bgmPLayer;
    }

    public static void PlaySfx(int audioID)
    {
        PlaySfx(audioID, Vector3.zero);
    }

    public static void PlaySfx(int audioID, Vector3 position)
    {
        GameObject player = Object.Instantiate(m_audioPLayerPrefab);

        AudioClip audioClip = m_sfxs[audioID];

        player.transform.position = position;
        player.GetComponent<AutoDestroyerAfterDuration>().Duration = audioClip.length;

        AudioSource source = player.GetComponent<AudioSource>();

        source.clip = audioClip;
        source.Play();
    }

    public static void PlayBGM(int id)
    {
        AudioClip bgm = Resources.Load<AudioClip>(Path.Combine(PATH_TO_BGM_FOLDER, id.ToString()));

        m_bgmPLayer.clip = bgm;
        m_bgmPLayer.Play();
    }

    public static void StopBGM()
    {
        m_bgmPLayer.Stop();
    }

}
