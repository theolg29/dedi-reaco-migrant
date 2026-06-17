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
    [Tooltip("Son du battement de cœur, joué en boucle naturelle dès la Phase 1 (le clip contient déjà son propre rythme)")]
    public AudioClip sonBattementCoeur;
    [Tooltip("Son de respiration, joué en boucle pendant toute la séquence")]
    public AudioClip sonRespiration;
    [Tooltip("Intervalle de base entre deux impulsions de vibration/shake (en secondes), dès la Phase 1 — se resserre automatiquement avec l'accélération du cœur (pitch)")]
    public float intervalleBattement = 0.9f;

    [Header("Phase 1 — Battement de cœur (fin du dialogue)")]
    [Tooltip("Volume du battement au tout début")]
    public float volumeBattementDepart = 0.1f;
    [Tooltip("Volume du battement visé en phases 1-2 (avant l'intensité max)")]
    public float volumeBattementMilieu = 0.35f;
    [Tooltip("Vitesse de montée du volume du battement — plus c'est haut, plus la transition est rapide")]
    public float vitesseMonteeBattement = 0.5f;
    [Tooltip("Vitesse de lecture (pitch) du cœur en phase 1 — 1 = rythme normal du clip")]
    public float pitchBattementDepart = 1f;

    [Header("Phase 2 — Respiration + vignette (sortie de la salle)")]
    [Tooltip("Volume de la respiration visé en phase 2 (avant l'intensité max)")]
    public float volumeRespirationMilieu = 0.4f;
    [Tooltip("Opacité de la vignette visée en phase 2")]
    public float opaciteVignetteMilieu = 0.25f;
    [Tooltip("Vitesse de montée de la respiration et de la vignette")]
    public float vitesseMonteeRespiration = 0.4f;
    [Tooltip("Vitesse de lecture (pitch) du cœur en phase 2 — le cœur bat plus vite")]
    public float pitchBattementPhase2 = 1.25f;
    [Tooltip("Intensité de la vibration en phase 2, avant le maximum")]
    public float intensiteVibrationPhase2 = 0.15f;

    [Header("Phase 3 — Intensité max (couloir s'allonge)")]
    [Tooltip("Vitesse à laquelle tous les effets montent vers leur maximum")]
    public float vitesseMonteeIntense = 0.3f;
    [Tooltip("Vitesse de lecture (pitch) du cœur en phase 3 — le cœur bat très rapidement")]
    public float pitchBattementMax = 1.6f;

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
    private float pitchBattementActuel;
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
        pitchBattementActuel = pitchBattementDepart;
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
        {
            sourceBattement.clip = sonBattementCoeur;
            sourceBattement.volume = volumeBattementActuel;
            sourceBattement.pitch = pitchBattementActuel;
            sourceBattement.Play();
        }

        StartCoroutine(BoucleHapticsEtShake());
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
        float ciblePitch = phase >= 3 ? pitchBattementMax : (phase >= 2 ? pitchBattementPhase2 : pitchBattementDepart);
        float cibleVibration = phase >= 3 ? intensiteVibration : (phase >= 2 ? intensiteVibrationPhase2 : intensiteVibrationPhase1);
        float vitBattement = phase >= 3 ? vitesseMonteeIntense : (phase >= 2 ? vitesseMonteeRespiration : vitesseMonteeBattement);

        volumeBattementActuel = Mathf.Lerp(volumeBattementActuel, cibleBattement, Time.deltaTime * vitBattement);
        pitchBattementActuel = Mathf.Lerp(pitchBattementActuel, ciblePitch, Time.deltaTime * vitBattement);
        intensiteVibrationActuelle = Mathf.Lerp(intensiteVibrationActuelle, cibleVibration, Time.deltaTime * vitBattement);
        sourceBattement.volume = volumeBattementActuel;
        sourceBattement.pitch = pitchBattementActuel;

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

    IEnumerator BoucleHapticsEtShake()
    {
        while (phase >= 1)
        {
            // Dès la phase 1, vibration légère synchronisée sur le battement, qui monte de phase en phase.
            // Le shake caméra reste réservé à la phase 3 (intensité max).
            if (manetteDroite.isValid)
                manetteDroite.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);
            if (manetteGauche.isValid)
                manetteGauche.SendHapticImpulse(0, intensiteVibrationActuelle, dureeVibration);

            if (phase >= 3 && cibleShake != null)
                StartCoroutine(Secouer(intensiteShakeActuelle));

            // Le cœur qui s'accélère (pitch) rapproche aussi les impulsions de vibration
            yield return new WaitForSeconds(intervalleBattement / Mathf.Max(0.01f, pitchBattementActuel));
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
