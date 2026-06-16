using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbianceCouloir : MonoBehaviour
{
    public static AmbianceCouloir Instance { get; private set; }

    [Tooltip("Son d'ambiance joué en boucle dans le couloir")]
    public AudioClip sonAmbiance;
    [Tooltip("Volume normal, dans le couloir")]
    public float volumeNormal = 1f;
    [Tooltip("Volume une fois étouffé par une porte fermée")]
    public float volumeEtouffe = 0.15f;

    private AudioSource source;
    private Coroutine fondu;

    void Awake()
    {
        Instance = this;

        source = GetComponent<AudioSource>();
        source.clip = sonAmbiance;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volumeNormal;
    }

    void Start()
    {
        source.Play();
    }

    // Appelé par une porte qui se ferme, avec sa propre durée d'animation
    public void Etouffer(float duree)
    {
        LancerFondu(volumeEtouffe, duree);
    }

    // Appelé par une porte qui s'ouvre, avec sa propre durée d'animation
    public void Retablir(float duree)
    {
        LancerFondu(volumeNormal, duree);
    }

    void LancerFondu(float cible, float duree)
    {
        if (fondu != null)
            StopCoroutine(fondu);
        fondu = StartCoroutine(AnimerVolume(cible, duree));
    }

    IEnumerator AnimerVolume(float cible, float duree)
    {
        float depart = source.volume;
        float t = 0f;
        while (t < duree)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(depart, cible, t / duree);
            yield return null;
        }
        source.volume = cible;
    }
}
