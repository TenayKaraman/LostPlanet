using UnityEngine;

namespace LostPlanet.Core
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource sfxSource;
        public AudioClip pickupSfx, shieldSfx, empSfx, phaseSfx, bombSfx, freezeSfx, portalOpenSfx;

        public void Init()
        {
            if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip && sfxSource) sfxSource.PlayOneShot(clip);
        }
    }
}
