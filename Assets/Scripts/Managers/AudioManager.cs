using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioPool))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioPool sfxPool;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("UI SFX")]
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        } 

        if (sfxPool != null)
            sfxPool.Init(sfxMixerGroup);
    }

    public void PlaySfx(SFX sfx, float volume = 1f)
    {
        if (sfxPool == null)
            return;

        AudioClip clip = null;
        
        switch (sfx)
        {
            case SFX.UI_Hover:
                clip = uiHover;
                break;
            case SFX.UI_Click:
                clip = uiClick;
                break;
        }

        if (clip == null)
            return;

        sfxPool.PlaySound(volume, clip, 1f);
    }
}
