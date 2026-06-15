using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PrinterAnimation : MonoBehaviour
{
    [Header("Références")]
    public Transform feuilleVierge;
    public Transform feuilleImprimee;
    public Renderer  voyantLED;

    [Header("Positions cibles — placer des GameObjects vides dans la scène")]
    [Tooltip("Là où paper-vierge doit arriver après aspiration (à l'intérieur de l'imprimante)")]
    public Transform cibleVierge;
    [Tooltip("Là où paper-print doit arriver en fin d'impression (complètement sorti)")]
    public Transform ciblePrint;

    [Header("Impression — coups réguliers (6.30s → 11.80s)")]
    public int   nombreCoups = 15;
    [Tooltip("Durée du lerp d'un seul coup")]
    public float dureeCoup   = 0.12f;

    [Header("Voyant LED")]
    public float vitesseClignotement = 1f;

    [Header("Feuille")]
    [Tooltip("Script sur la feuille imprimée pour activer la récupération")]
    public FeuilleRecuperable feuilleRecuperable;

    [Header("Audio")]
    public AudioClip sonImpression;

    // Timings calés sur le son (secondes)
    const float T_DEBUT_ASPIRATION = 4.40f;
    const float T_FIN_ASPIRATION   = 5.50f;
    const float T_DEBUT_IMPRESSION = 6.30f;
    const float T_DEBUT_GROS_ACOUP = 11.80f;
    const float T_FIN              = 12.30f;

    private AudioSource sourceAudio;
    private bool        clignotementActif = false;

    void Awake()
    {
        sourceAudio = GetComponent<AudioSource>();
        sourceAudio.playOnAwake = false;
        if (voyantLED != null) voyantLED.enabled = true;
    }

    public void Imprimer()
    {
        StartCoroutine(AnimationImpression());
    }

    IEnumerator AnimationImpression()
    {
        if (sonImpression != null)
        {
            sourceAudio.clip = sonImpression;
            sourceAudio.Play();
        }

        // Phase 1 — 0s → 4.40s : démarrage, LED clignote uniquement
        clignotementActif = true;
        StartCoroutine(ClignoteVoyant());
        yield return new WaitForSeconds(T_DEBUT_ASPIRATION);

        // Phase 2 — 4.40s → 5.50s : aspiration de paper-vierge dans l'imprimante
        if (cibleVierge != null)
            yield return StartCoroutine(DeplacerVers(feuilleVierge, cibleVierge.position, T_FIN_ASPIRATION - T_DEBUT_ASPIRATION));
        else
            yield return new WaitForSeconds(T_FIN_ASPIRATION - T_DEBUT_ASPIRATION);

        // Phase 3 — 5.50s → 6.30s : pause
        yield return new WaitForSeconds(T_DEBUT_IMPRESSION - T_FIN_ASPIRATION);

        // Phase 4 — 6.30s → 11.80s : 15 coups réguliers, paper-print sort progressivement
        if (ciblePrint != null)
        {
            Vector3 posDepart      = feuilleImprimee.position;
            float   dureePhase     = T_DEBUT_GROS_ACOUP - T_DEBUT_IMPRESSION;
            float   pauseEntreCoup = Mathf.Max(0f, (dureePhase / nombreCoups) - dureeCoup);
            for (int i = 0; i < nombreCoups; i++)
            {
                float   t     = (float)(i + 1) / nombreCoups;
                Vector3 cible = Vector3.Lerp(posDepart, ciblePrint.position, t * 0.85f); // 85% du chemin en coups réguliers
                yield return StartCoroutine(DeplacerVers(feuilleImprimee, cible, dureeCoup));
                yield return new WaitForSeconds(pauseEntreCoup);
            }
        }
        else
            yield return new WaitForSeconds(T_DEBUT_GROS_ACOUP - T_DEBUT_IMPRESSION);

        // Phase 5 — 11.80s → 12.30s : gros à-coup final jusqu'à la position finale
        if (ciblePrint != null)
            yield return StartCoroutine(DeplacerVers(feuilleImprimee, ciblePrint.position, T_FIN - T_DEBUT_GROS_ACOUP));
        else
            yield return new WaitForSeconds(T_FIN - T_DEBUT_GROS_ACOUP);

        // Phase 6 — fin : LED s'éteint fixe, feuille récupérable
        clignotementActif = false;
        if (voyantLED != null) voyantLED.enabled = true;

        if (feuilleRecuperable != null)
            feuilleRecuperable.ActiverRecuperation();
    }

    IEnumerator DeplacerVers(Transform feuille, Vector3 destination, float duree)
    {
        Vector3 depart = feuille.position;
        float   t      = 0f;
        while (t < duree)
        {
            t += Time.deltaTime;
            feuille.position = Vector3.Lerp(depart, destination, Mathf.Clamp01(t / duree));
            yield return null;
        }
        feuille.position = destination;
    }

    IEnumerator ClignoteVoyant()
    {
        if (voyantLED == null) yield break;
        while (clignotementActif)
        {
            voyantLED.enabled = !voyantLED.enabled;
            yield return new WaitForSeconds(vitesseClignotement);
        }
    }
}
