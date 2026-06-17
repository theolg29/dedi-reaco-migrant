using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FanRotator : MonoBehaviour
{
    public enum AxeRotation { X, Y, Z }

    [Header("Rotation")]
    [Tooltip("Axe local autour duquel le ventilateur tourne")]
    public AxeRotation axeRotation = AxeRotation.Z;
    [Tooltip("Vitesse de rotation en degrés par seconde")]
    public float vitesseRotation = 360f;

    [Header("Son")]
    [Tooltip("Son du ventilateur joué en boucle pendant la rotation")]
    public AudioClip sonVentilateur;
    [Tooltip("Volume du son du ventilateur quand la porte est ouverte")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Tooltip("Porte de la salle — le son est étouffé quand elle est fermée")]
    public DoorInteractable porte;
    [Tooltip("Volume du son une fois étouffé par la porte fermée")]
    [Range(0f, 1f)]
    public float volumeEtouffe = 0.05f;

    private AudioSource sourceAudio;
    private Coroutine fondu;

    void Awake()
    {
        sourceAudio = GetComponent<AudioSource>();
        sourceAudio.clip = sonVentilateur;
        sourceAudio.loop = true;
        sourceAudio.playOnAwake = false;
        sourceAudio.volume = porte != null && !porte.EstOuverte ? volumeEtouffe : volume;
        sourceAudio.spatialBlend = 1f;

        if (porte != null)
            porte.Animee += OnPorteAnimee;
    }

    void OnDestroy()
    {
        if (porte != null)
            porte.Animee -= OnPorteAnimee;
    }

    void OnPorteAnimee(bool ouverte, float duree)
    {
        if (fondu != null)
            StopCoroutine(fondu);
        fondu = StartCoroutine(AnimerVolume(ouverte ? volume : volumeEtouffe, duree));
    }

    IEnumerator AnimerVolume(float cible, float duree)
    {
        float depart = sourceAudio.volume;
        float t = 0f;
        while (t < duree)
        {
            t += Time.deltaTime;
            sourceAudio.volume = Mathf.Lerp(depart, cible, t / duree);
            yield return null;
        }
        sourceAudio.volume = cible;
    }

    void Start()
    {
        if (sonVentilateur != null)
            sourceAudio.Play();
    }

    void Update()
    {
        Vector3 axe = axeRotation switch
        {
            AxeRotation.X => Vector3.right,
            AxeRotation.Y => Vector3.up,
            _ => Vector3.forward
        };
        transform.Rotate(axe, vitesseRotation * Time.deltaTime, Space.Self);
    }
}
