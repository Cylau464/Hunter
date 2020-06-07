using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager current;

    [Header("Ambient Audio")]
    public AudioClip ambientClip;		//The background ambient sound
    public AudioClip musicClip;			//The background music 

    [Header("Player Audio")]
    public AudioClip[] walkStepClips;
    public AudioClip jump;
    public AudioClip doubleJump;
    public AudioClip evade;
    public AudioClip[] hurt;
    public AudioClip healing;
    public AudioClip healUp;
    public AudioClip death;

    [Header("Attack Audio")]
    public AudioClip[] attackImpact;

    [Header("Mixer Groups")]
    public AudioMixerGroup ambientGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup playerGroup;
    public AudioMixerGroup particlesGroup;
    public AudioMixerGroup SFXGroup;

    public AudioSource playerSource;
    AudioSource ambientSource;
    AudioSource musicSource;
    AudioSource particlesSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (current != null && current != this)
        {
            //...destroy this. There can be only one AudioManager
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        ambientSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        musicSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        playerSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        particlesSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        sfxSource = gameObject.AddComponent<AudioSource>() as AudioSource;

        ambientSource.outputAudioMixerGroup = ambientGroup;
        musicSource.outputAudioMixerGroup = musicGroup;
        playerSource.outputAudioMixerGroup = playerGroup;
        particlesSource.outputAudioMixerGroup = particlesGroup;
        sfxSource.outputAudioMixerGroup = SFXGroup;

        StartLevelAudio();
    }

    private void Update()
    {
        if (!current.sfxSource.isPlaying)
            current.sfxSource.volume = 1f;
    }

    void StartLevelAudio()
    {
        //Set the clip for ambient audio, tell it to loop, and then tell it to play
        ambientSource.clip = current.ambientClip;
        ambientSource.loop = true;
        ambientSource.Play();

        //Set the clip for music audio, tell it to loop, and then tell it to play
        current.musicSource.clip = current.musicClip;
        current.musicSource.loop = true;
        current.musicSource.Play();
    }

    public static void PlayFootstepAudio()
    {
        //If there is no current AudioManager or the player source is already playing
        //a clip, exit 
        if (current == null || current.playerSource.isPlaying)
            return;

        //Pick a random footstep sound
        int index = Random.Range(0, current.walkStepClips.Length);

        //Set the footstep clip and tell the source to play
        current.playerSource.pitch = 1f;
        current.playerSource.clip = current.walkStepClips[index];
        current.playerSource.Play();
    }

    public static void PlayEvadeAudio()
    {
        if (current == null)
            return;

        //Set the footstep clip and tell the source to play
        //current.sfxSource.clip = current.evade;
        //current.sfxSource.volume = .25f;
        current.playerSource.pitch = 1f;
        current.playerSource.PlayOneShot(current.evade);
    }

    public static void PlayJumpAudio()
    {
        if (current == null)
            return;

        //Set the footstep clip and tell the source to play
        current.playerSource.pitch = 1f;
        current.playerSource.PlayOneShot(current.jump, 1f);
    }

    public static void PlayDoubleJumpAudio()
    {
        if (current == null)
            return;

        current.playerSource.pitch = Random.Range(.5f, 1f);
        //Set the footstep clip and tell the source to play
        current.playerSource.PlayOneShot(current.doubleJump, .5f);
    }

    public static void PlayHurtAudio()
    {
        if (current == null || current.playerSource.isPlaying)
            return;

        current.playerSource.pitch = Random.Range(.85f, 1f);
        current.playerSource.PlayOneShot(current.hurt[Random.Range(0, current.hurt.Length)]);
    }

    public static void PlayDeathAudio()
    {
        if (current == null)
            return;

        current.playerSource.pitch = Random.Range(.85f, 1f);
        current.playerSource.PlayOneShot(current.death);
    }

    public static void PlaySwingAudio(AudioClip clip)
    {
        if (current == null)
            return;

        current.sfxSource.PlayOneShot(clip, .5f);
    }

    public static void PlayAttackAudio(AudioClip clip)
    {
        if (current == null)
            return;

        current.sfxSource.PlayOneShot(clip, .5f);
    }

    public static void PlayHealingAudio()
    {
        if (current == null)
            return;

        current.playerSource.PlayOneShot(current.healing);
    }

    public static void PlayHealUpAudio()
    {
        if (current == null)
            return;

        current.playerSource.PlayOneShot(current.healUp, .25f);
    }

    public static void PlayCastSpellAudio(AudioClip clip)
    {
        if (current == null)
            return;

        current.playerSource.clip = clip;
        current.playerSource.Play();
    }

    public static void StopPlayerAudio()
    {
        if (current == null)
            return;

        current.playerSource.Stop();
        current.playerSource.clip = null;
    }

    public static AudioSource PlayClipAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 1f, float dopplerLevel = 1f, Transform parent = null)
    {
        GameObject _go = new GameObject("One Shot Audio");
        _go.transform.position = position;
        _go.transform.parent = parent;
        AudioSource _as = _go.AddComponent<AudioSource>();
        _as.clip = clip;
        _as.volume = volume;
        _as.spatialBlend = 1f;
        _as.minDistance = minDistance;
        _as.dopplerLevel = dopplerLevel;
        _as.Play();
        Destroy(_go, _as.clip.length);

        return _as;

    }
}
