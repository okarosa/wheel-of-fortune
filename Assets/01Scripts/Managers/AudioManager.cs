using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AudioEntry
{
    public string id;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Entries")]
    [SerializeField] private List<AudioEntry> entries;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;
    private Dictionary<string, AudioClip> _clipMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameServices.Instance?.Register(this);
        DontDestroyOnLoad(gameObject);

        _clipMap = new Dictionary<string, AudioClip>();
        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.id) && e.clip != null && !_clipMap.ContainsKey(e.id))
                    _clipMap.Add(e.id, e.clip);
            }
        }

        var sfxGO = new GameObject("sfx_audiosource");
        sfxGO.transform.SetParent(transform);
        _sfxSource = sfxGO.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        var musicGO = new GameObject("music_audiosource");
        musicGO.transform.SetParent(transform);
        _musicSource = musicGO.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
    }

    public void Play(string id)
    {
        if (_clipMap == null || !_clipMap.TryGetValue(id, out var clip) || clip == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (_musicSource.isPlaying && _musicSource.clip == clip) return;
        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }
}
