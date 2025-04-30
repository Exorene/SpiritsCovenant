using UnityEngine;

public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance { get; private set; }

    [SerializeField] private AudioClip bgmClip;
    private AudioSource _audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = bgmClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
