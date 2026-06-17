using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Un seul script pour tous les sons de couloir : fondu de volume à chaque porte (n'importe
// laquelle), plus étouffement optionnel par mur si calqueMurs est configuré (sinon ignoré).
public class DoorMuffledVolume : MonoBehaviour
{
    [Tooltip("Sources audio gérées (discussion, ambiance, sons de détail...)")]
    public AudioSource[] sources;

    [Header("Fondu sur porte")]
    [Tooltip("Volume normal, porte ouverte")]
    public float volumeNormal = 1f;
    [Tooltip("Volume une fois une porte fermée quelque part — quasi inaudible")]
    public float volumeEtouffe = 0.1f;

    [Header("Occlusion par mur (optionnel — laisser sur \"Nothing\" si pas besoin)")]
    [Tooltip("Calque des murs à détecter entre la caméra et chaque source")]
    public LayerMask calqueMurs;
    [Tooltip("Fréquence de coupure du filtre passe-bas quand un mur bloque le son")]
    public float frequenceEtouffeeParMur = 800f;
    [Tooltip("Fréquence de coupure quand rien ne bloque (son normal)")]
    public float frequenceNormale = 22000f;

    private static readonly List<DoorMuffledVolume> instances = new List<DoorMuffledVolume>();

    private AudioLowPassFilter[] filtres;
    private Coroutine[] fondus;

    void Awake()
    {
        filtres = new AudioLowPassFilter[sources.Length];
        fondus = new Coroutine[sources.Length];

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;
            sources[i].volume = volumeNormal;

            if (calqueMurs != 0)
            {
                var filtre = sources[i].GetComponent<AudioLowPassFilter>();
                if (filtre == null)
                    filtre = sources[i].gameObject.AddComponent<AudioLowPassFilter>();
                filtre.cutoffFrequency = frequenceNormale;
                filtres[i] = filtre;
            }
        }
    }

    void OnEnable() => instances.Add(this);
    void OnDisable() => instances.Remove(this);

    void Update()
    {
        if (calqueMurs == 0 || Camera.main == null) return;
        Vector3 origine = Camera.main.transform.position;

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null || filtres[i] == null) continue;

            Vector3 versSource = sources[i].transform.position - origine;
            bool occulteParMur = Physics.Raycast(origine, versSource.normalized, versSource.magnitude, calqueMurs);
            filtres[i].cutoffFrequency = occulteParMur ? frequenceEtouffeeParMur : frequenceNormale;
        }
    }

    // Appelées par DoorInteractable à chaque porte fermée/ouverte, pour toutes les instances en même temps
    public static void EtoufferToutes(float duree)
    {
        foreach (var instance in instances)
            instance.LancerFondus(instance.volumeEtouffe, duree);
    }

    public static void RetablirToutes(float duree)
    {
        foreach (var instance in instances)
            instance.LancerFondus(instance.volumeNormal, duree);
    }

    void LancerFondus(float cible, float duree)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;
            if (fondus[i] != null)
                StopCoroutine(fondus[i]);
            fondus[i] = StartCoroutine(AnimerVolume(sources[i], cible, duree));
        }
    }

    IEnumerator AnimerVolume(AudioSource source, float cible, float duree)
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
