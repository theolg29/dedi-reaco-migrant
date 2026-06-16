using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

[RequireComponent(typeof(BoxCollider))]
public class HallwayPanicEffect : MonoBehaviour
{
    [Header("Objets qui disparaissent (perte de tête)")]
    [Tooltip("Objets désactivés un par un, dans un ordre aléatoire, au fil du temps — démarré via DemarrerDisparitionObjets()")]
    public GameObject[] objetsADisparaitre;
    [Tooltip("Durée totale sur laquelle les objets disparaissent, en secondes")]
    public float dureeDisparitionObjets = 15f;

    [Header("Sons")]
    [Tooltip("Son du battement de cœur, rejoué à chaque pulsation")]
    public AudioClip sonBattementCoeur;
    [Tooltip("Son de respiration, joué en boucle pendant toute la séquence")]
    public AudioClip sonRespiration;
    [Tooltip("Intervalle entre deux battements, en secondes (à caler sur le rythme du son)")]
    public float intervalleBattement = 0.9f;
    [Tooltip("Volume du battement tout au début, avant la montée en intensité")]
    public float volumeBattementDepart = 0.2f;

    [Header("Montée en intensité")]
    [Tooltip("Durée sur laquelle le shake, la vibration, le volume du battement et la vignette montent progressivement jusqu'à leur valeur max")]
    public float dureeMontee = 20f;

    [Header("Shake caméra")]
    [Tooltip("Objet à secouer à chaque battement — assigner le \"Camera Offset\" du XR Origin, jamais la caméra elle-même (écrasée par le tracking)")]
    public Transform cibleShake;
    [Tooltip("Amplitude max du shake, atteinte en fin de montée")]
    public float intensiteShake = 0.03f;
    [Tooltip("Durée du shake à chaque battement")]
    public float dureeShake = 0.12f;

    [Header("Vibration manettes")]
    [Tooltip("Intensité max de la vibration (0-1), atteinte en fin de montée")]
    public float intensiteVibration = 0.5f;
    [Tooltip("Durée de la vibration à chaque battement")]
    public float dureeVibration = 0.1f;

    [Header("Vision réduite")]
    [Tooltip("Opacité maximale du voile noir, atteinte en fin de montée")]
    public float opaciteVignetteMax = 0.85f;

    private bool actif;
    private float tempsMontee;
    private float intensiteShakeActuelle;
    private float intensiteVibrationActuelle;
    private float volumeBattementActuel;

    private bool disparitionActive;
    private float[] seuilsDisparition;
    private int indexProchainObjet;
    private float tempsDisparition;

    private Vector3 posDepartShake;

    private AudioSource sourceBattement;
    private AudioSource sourceRespiration;
    private InputDevice manetteGauche;
    private InputDevice manetteDroite;

    private Image vignette;
    private Transform vignetteTransform;

    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        sourceBattement = gameObject.AddComponent<AudioSource>();
        sourceBattement.playOnAwake = false;

        sourceRespiration = gameObject.AddComponent<AudioSource>();
        sourceRespiration.playOnAwake = false;
        sourceRespiration.loop = true;
        sourceRespiration.clip = sonRespiration;

        CreerVignette();
    }

    void Start()
    {
        if (cibleShake != null)
            posDepartShake = cibleShake.localPosition;

        PreparerDisparitionObjets();
    }

    void CreerVignette()
    {
        var goCanvas = new GameObject("VignettePanique");
        goCanvas.transform.SetParent(transform, false);

        var canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 998;

        var rt = goCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(4000f, 4000f);
        rt.localScale = Vector3.one * 0.001f;

        var goImage = new GameObject("Image");
        goImage.transform.SetParent(goCanvas.transform, false);

        vignette = goImage.AddComponent<Image>();
        vignette.color = new Color(0f, 0f, 0f, 0f);

        var imgRt = goImage.GetComponent<RectTransform>();
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.sizeDelta = Vector2.zero;

        vignetteTransform = goCanvas.transform;
    }

    void PreparerDisparitionObjets()
    {
        if (objetsADisparaitre == null || objetsADisparaitre.Length == 0) return;

        // Mélange aléatoire (Fisher-Yates) pour un ordre de disparition imprévisible
        for (int i = objetsADisparaitre.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (objetsADisparaitre[i], objetsADisparaitre[j]) = (objetsADisparaitre[j], objetsADisparaitre[i]);
        }

        seuilsDisparition = new float[objetsADisparaitre.Length];
        for (int i = 0; i < seuilsDisparition.Length; i++)
            seuilsDisparition[i] = (i + 1f) / (seuilsDisparition.Length + 1f);
    }

    // Appelé depuis BureauManager (salle 01) dès l'interaction avec l'avatar
    public void DemarrerDisparitionObjets()
    {
        if (disparitionActive) return;
        disparitionActive = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (actif) return;

        actif = true;
        ObtenirManettes();

        if (sonRespiration != null)
            sourceRespiration.Play();

        StartCoroutine(BoucleBattementCoeur());
    }

    void Update()
    {
        // Le voile reste plaqué devant la caméra à chaque frame, jamais parenté directement à elle
        if (vignetteTransform != null && Camera.main != null)
        {
            Camera cam = Camera.main;
            vignetteTransform.position = cam.transform.position + cam.transform.forward * (cam.nearClipPlane + 0.05f);
            vignetteTransform.rotation = cam.transform.rotation;
        }

        if (actif)
        {
            tempsMontee += Time.deltaTime;
            float t = Mathf.Clamp01(tempsMontee / dureeMontee);

            intensiteShakeActuelle = Mathf.Lerp(0f, intensiteShake, t);
            intensiteVibrationActuelle = Mathf.Lerp(0f, intensiteVibration, t);
            volumeBattementActuel = Mathf.Lerp(volumeBattementDepart, 1f, t);

            Color c = vignette.color;
            c.a = Mathf.Lerp(0f, opaciteVignetteMax, t);
            vignette.color = c;
        }

        if (disparitionActive)
        {
            tempsDisparition += Time.deltaTime;
            float t = Mathf.Clamp01(tempsDisparition / dureeDisparitionObjets);

            while (seuilsDisparition != null && indexProchainObjet < seuilsDisparition.Length
                   && t >= seuilsDisparition[indexProchainObjet])
            {
                objetsADisparaitre[indexProchainObjet].SetActive(false);
                indexProchainObjet++;
            }
        }
    }

    void ObtenirManettes()
    {
        var droite = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, droite);
        if (droite.Count > 0) manetteDroite = droite[0];

        var gauche = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, gauche);
        if (gauche.Count > 0) manetteGauche = gauche[0];
    }

    IEnumerator BoucleBattementCoeur()
    {
        while (actif)
        {
            if (sonBattementCoeur != null)
                sourceBattement.PlayOneShot(sonBattementCoeur, volumeBattementActuel);

            if (manetteDroite.isValid)
                manetteDroite.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);
            if (manetteGauche.isValid)
                manetteGauche.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);

            if (cibleShake != null)
                StartCoroutine(Secouer(intensiteShakeActuelle));

            yield return new WaitForSeconds(intervalleBattement);
        }
    }

    IEnumerator Secouer(float intensite)
    {
        float t = 0f;
        while (t < dureeShake)
        {
            t += Time.deltaTime;
            float attenuation = 1f - (t / dureeShake);
            cibleShake.localPosition = posDepartShake + Random.insideUnitSphere * intensite * attenuation;
            yield return null;
        }
        cibleShake.localPosition = posDepartShake;
    }
}
