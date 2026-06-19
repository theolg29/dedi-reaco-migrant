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
    [Tooltip("Son du battement de cœur : une ambiance longue (~10-15s), jouée en boucle en continu dès la Phase 1 — son propre rythme reste fixe, seul le volume monte selon la phase")]
    public AudioClip sonBattementCoeur;
    [Tooltip("Son de respiration, joué en boucle pendant toute la séquence")]
    public AudioClip sonRespiration;

    [Header("Rythme du cœur — intervalle entre deux battements (secondes)")]
    [Tooltip("Intervalle visé en Phase 1 — rythme calme (ex: 1s ≈ 60 battements/min)")]
    public float intervalleBattementDepart = 1f;
    [Tooltip("Intervalle visé en Phase 2 — le cœur s'accélère (ex: 0.6s ≈ 100 battements/min)")]
    public float intervalleBattementPhase2 = 0.6f;
    [Tooltip("Intervalle visé en Phase 3 — panique totale (ex: 0.35s ≈ 170 battements/min)")]
    public float intervalleBattementMax = 0.35f;

    [Header("Phase 1 — Battement de cœur (fin du dialogue)")]
    [Tooltip("Volume du battement au tout début")]
    public float volumeBattementDepart = 0.1f;
    [Tooltip("Volume du battement visé en phases 1-2 (avant l'intensité max)")]
    public float volumeBattementMilieu = 0.35f;
    [Tooltip("Vitesse de montée du volume/rythme du battement — plus c'est haut, plus la transition est rapide")]
    public float vitesseMonteeBattement = 0.5f;

    [Header("Phase 2 — Respiration + vignette (sortie de la salle)")]
    [Tooltip("Volume de la respiration visé en phase 2 (avant l'intensité max)")]
    public float volumeRespirationMilieu = 0.4f;
    [Tooltip("Opacité de la vignette visée en phase 2")]
    public float opaciteVignetteMilieu = 0.25f;
    [Tooltip("Vitesse de montée de la respiration et de la vignette")]
    public float vitesseMonteeRespiration = 0.4f;
    [Tooltip("Intensité de la vibration en phase 2, avant le maximum")]
    public float intensiteVibrationPhase2 = 0.15f;

    [Header("Phase 3 — Intensité max (couloir s'allonge)")]
    [Tooltip("Vitesse à laquelle tous les effets montent vers leur maximum")]
    public float vitesseMonteeIntense = 0.3f;

    [Header("Shake caméra")]
    [Tooltip("Objet à secouer à chaque battement — assigner le \"Camera Offset\" du XR Origin, jamais la caméra elle-même (écrasée par le tracking)")]
    public Transform cibleShake;
    [Tooltip("Amplitude max du shake, atteinte en phase 3")]
    public float intensiteShake = 0.03f;
    [Tooltip("Durée du shake à chaque battement")]
    public float dureeShake = 0.12f;

    [Header("Vibration manettes")]
    [Tooltip("Intensité max de la vibration (0-1), atteinte en phase 3")]
    public float intensiteVibration = 0.5f;
    [Tooltip("Intensité très légère de la vibration dès la Phase 1, synchronisée sur le battement de cœur")]
    public float intensiteVibrationPhase1 = 0.05f;
    [Tooltip("Durée de la vibration à chaque battement")]
    public float dureeVibration = 0.1f;

    [Header("Vision réduite")]
    [Tooltip("Opacité maximale du voile noir, atteinte en phase 3")]
    public float opaciteVignetteMax = 0.85f;
    [Tooltip("Amplitude du scintillement de la vignette")]
    public float amplitudeFlickerVignette = 0.1f;
    [Tooltip("Vitesse du scintillement de la vignette")]
    public float vitesseFlickerVignette = 1.5f;

    private int phase;
    private float volumeBattementActuel;
    private float intervalleBattementActuel;
    private float opaciteVignetteBase;
    private float intensiteShakeActuelle;
    private float intensiteVibrationActuelle;

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
        sourceBattement.loop = true;
        sourceBattement.clip = sonBattementCoeur;

        sourceRespiration = gameObject.AddComponent<AudioSource>();
        sourceRespiration.playOnAwake = false;
        sourceRespiration.loop = true;
        sourceRespiration.clip = sonRespiration;
        sourceRespiration.volume = 0f;

        CreerVignette();
    }

    void Start()
    {
        if (cibleShake != null)
            posDepartShake = cibleShake.localPosition;

        PreparerDisparitionObjets();
        volumeBattementActuel = volumeBattementDepart;
        intervalleBattementActuel = intervalleBattementDepart;
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

    // Appelé depuis BureauManager dès l'interaction avec l'avatar
    public void DemarrerDisparitionObjets()
    {
        if (disparitionActive) return;
        disparitionActive = true;
    }

    /// Phase 1 — Battement de cœur (appelé par BureauManager vers la fin du dialogue)
    public void DemarrerBattementCoeur()
    {
        if (phase >= 1) return;
        phase = 1;
        ObtenirManettes();

        if (sonBattementCoeur != null)
            sourceBattement.Play();

        StartCoroutine(BoucleBattement());
    }

    /// Phase 2 — Respiration + vignette (déclenché quand le joueur entre dans le couloir)
    public void DemarrerRespiration()
    {
        if (phase < 1 || phase >= 2) return;
        phase = 2;
        if (sonRespiration != null)
            sourceRespiration.Play();
    }

    /// Phase 3 — Tout au maximum (appelé par WallSlide quand le couloir s'allonge)
    public void DemarrerIntensiteMax()
    {
        if (phase < 1 || phase >= 3) return;
        if (phase < 2)
            DemarrerRespiration();
        phase = 3;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (phase < 1) return;
        DemarrerRespiration();
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

        // Disparition progressive des objets
        if (disparitionActive)
        {
            tempsDisparition += Time.deltaTime;
            float tDisp = Mathf.Clamp01(tempsDisparition / dureeDisparitionObjets);

            while (seuilsDisparition != null && indexProchainObjet < seuilsDisparition.Length
                   && tDisp >= seuilsDisparition[indexProchainObjet])
            {
                objetsADisparaitre[indexProchainObjet].SetActive(false);
                indexProchainObjet++;
            }
        }

        if (phase < 1) return;

        // ── Phase 1+ : battement de cœur monte progressivement, de plus en plus vite à chaque phase ──
        float cibleBattement = phase >= 3 ? 1f : volumeBattementMilieu;
        float cibleIntervalle = phase >= 3 ? intervalleBattementMax : (phase >= 2 ? intervalleBattementPhase2 : intervalleBattementDepart);
        float cibleVibration = phase >= 3 ? intensiteVibration : (phase >= 2 ? intensiteVibrationPhase2 : intensiteVibrationPhase1);
        float vitBattement = phase >= 3 ? vitesseMonteeIntense : (phase >= 2 ? vitesseMonteeRespiration : vitesseMonteeBattement);

        volumeBattementActuel = Mathf.Lerp(volumeBattementActuel, cibleBattement, Time.deltaTime * vitBattement);
        intervalleBattementActuel = Mathf.Lerp(intervalleBattementActuel, cibleIntervalle, Time.deltaTime * vitBattement);
        intensiteVibrationActuelle = Mathf.Lerp(intensiteVibrationActuelle, cibleVibration, Time.deltaTime * vitBattement);
        sourceBattement.volume = volumeBattementActuel;

        // ── Phase 2+ : respiration + vignette ──
        if (phase >= 2)
        {
            float cibleRespiration = phase >= 3 ? 1f : volumeRespirationMilieu;
            float cibleVignette = phase >= 3 ? opaciteVignetteMax : opaciteVignetteMilieu;
            float vitRespiration = phase >= 3 ? vitesseMonteeIntense : vitesseMonteeRespiration;

            sourceRespiration.volume = Mathf.Lerp(sourceRespiration.volume, cibleRespiration, Time.deltaTime * vitRespiration);
            opaciteVignetteBase = Mathf.Lerp(opaciteVignetteBase, cibleVignette, Time.deltaTime * vitRespiration);

            // Scintillement proportionnel à l'opacité pour un effet vivant
            float flickerT = Mathf.Clamp01(opaciteVignetteBase / opaciteVignetteMax);
            float flicker = (Mathf.PerlinNoise(Time.time * vitesseFlickerVignette, 0f) - 0.5f)
                            * 2f * amplitudeFlickerVignette * flickerT;

            Color c = vignette.color;
            c.a = Mathf.Clamp01(opaciteVignetteBase + flicker);
            vignette.color = c;
        }

        // ── Phase 3 : le shake caméra monte vers le max (réservé à l'intensité maximale) ──
        if (phase >= 3)
            intensiteShakeActuelle = Mathf.Lerp(intensiteShakeActuelle, intensiteShake, Time.deltaTime * vitesseMonteeIntense);
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

    IEnumerator BoucleBattement()
    {
        while (phase >= 1)
        {
            // Vibration + shake déclenchés au rythme du cœur ; le son (ambiance longue en boucle) tourne en continu, indépendamment de ce rythme.
            // Le shake caméra reste réservé à la phase 3 (intensité max).
            if (manetteDroite.isValid)
                manetteDroite.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);
            if (manetteGauche.isValid)
                manetteGauche.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);

            if (phase >= 3 && cibleShake != null)
                StartCoroutine(Secouer(intensiteShakeActuelle));

            yield return new WaitForSeconds(intervalleBattementActuel);
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
