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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlaySfx(Sfx sfx, float volume = 1f)
    {
        if (sfxPool == null)
            return;

        AudioClip clip = null;
        
        switch (sfx)
        {
            case Sfx.UIHover:
                clip = uiHover;
                break;
            case Sfx.UIClick:
                clip = uiClick;
                break;
        }

        if (clip == null)
            return;

        sfxPool.PlaySound(volume, clip, 1f);
    }
}
