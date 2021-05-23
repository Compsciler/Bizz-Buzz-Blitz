using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] AudioClip[] clickSounds;

    public void PlaySound(int soundNum)
    {
        AudioManager.instance.SFX_Source.PlayOneShot(clickSounds[soundNum]);
    }
    public void PlaySound(AudioClip sound)
    {
        AudioManager.instance.SFX_Source.PlayOneShot(sound);
    }
    public void PlaySound(AudioClip sound, float volumeScale)
    {
        AudioManager.instance.SFX_Source.PlayOneShot(sound, volumeScale);
    }
    public void PlaySound(AudioClip sound, float volumeScale, float pitch)
    {
        AudioManager.instance.SFX_SourceVariablePitch.pitch = pitch;
        AudioManager.instance.SFX_SourceVariablePitch.PlayOneShot(sound, volumeScale);
    }
}