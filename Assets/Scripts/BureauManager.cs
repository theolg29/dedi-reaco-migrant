using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BureauManager : MonoBehaviour
{
    [Header("Avatar")]
    [Tooltip("L'avatar sur lequel appuyer la gâchette pour démarrer la salle — contient l'AudioSource du dialogue")]
    public XRSimpleInteractable avatar;
    [Tooltip("Optionnel — Timeline à lancer au démarrage de la salle (ex: animation de l'avatar)")]
    public PlayableDirector timeline;

    [Header("Imprimante")]
    [Tooltip("Optionnel — se lance automatiquement en fin de dialogue")]
    public PrinterAnimation imprimante;
    [Tooltip("Délai après le début du dialogue avant de lancer l'impression, en secondes")]
    public float delaiAvantImpression = 0f;
    [Tooltip("Si coché, simule un bourrage papier (son différent, la feuille ne sort jamais) au lieu d'imprimer normalement")]
    public bool bourragePapier = false;

    [Header("Porte")]
    [Tooltip("La porte de cette salle")]
    public DoorInteractable porte;
    [Tooltip("La porte de la salle suivante à déverrouiller une fois cette salle validée")]
    public DoorInteractable porteSuivante;

    [Header("Changement d'éclairage (optionnel)")]
    [Tooltip("Optionnel — bascule entre deux GameObjects (ex: deux setups d'éclairage) à la fin du dialogue")]
    public GameObjectSwapper changementLumiere;

    [Header("Effet couloir (salle B02 uniquement)")]
    [Tooltip("Optionnel — déclenche la disparition des objets dès l'interaction avec l'avatar, puis le battement de cœur après le délai ci-dessous (la respiration et l'intensité max se déclenchent ensuite dans le couloir)")]
    public HallwayPanicEffect effetCouloir;
    [Tooltip("Délai après le début du dialogue avant de déclencher le battement de cœur, en secondes")]
    public float delaiAvantBattementCoeur = 0f;

    [Header("Dialogues")]
    [Tooltip("Canvas WorldSpace positionné statiquement devant le joueur")]
    public GameObject panneauDialogue;
    [Tooltip("Durée du fade in/out en secondes")]
    public float dureeTransition = 0.4f;
    [Tooltip("Texte du panneau de dialogue")]
    public TextMeshProUGUI texteDialogue;

    [System.Serializable]
    public struct LigneDialogue
    {
        [TextArea] public string texte;
        [Tooltip("Durée d'affichage en secondes")]
        public float duree;
    }

    [Tooltip("Séquence de textes à afficher pendant le son")]
    public LigneDialogue[] dialogues;

    private CanvasGroup groupeDialogue;
    private AudioSource sourceAudioDialogue;
    private InputDevice manetteDroite;
    private bool gachettePrecedente;
    private bool survolAvatarActif;
    private bool salleDemarree;
    private bool salleValidee;
    private bool dialogueTermine;

    void Awake()
    {
        if (avatar != null)
            sourceAudioDialogue = avatar.GetComponent<AudioSource>();

        if (panneauDialogue != null)
        {
            groupeDialogue = panneauDialogue.GetComponent<CanvasGroup>();
            if (groupeDialogue == null)
                groupeDialogue = panneauDialogue.AddComponent<CanvasGroup>();
            groupeDialogue.alpha = 0f;
        }

        if (porteSuivante != null)
            porteSuivante.Verrouiller();

        if (avatar != null)
        {
            avatar.hoverEntered.AddListener(_ => survolAvatarActif = true);
            avatar.hoverExited.AddListener(_ => survolAvatarActif = false);
        }
    }

    void Update()
    {
        if (salleDemarree) return;

        if (!manetteDroite.isValid)
        {
            var dispositifs = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, dispositifs);
            if (dispositifs.Count > 0)
                manetteDroite = dispositifs[0];
        }

        float valeurGachette = 0f;
        if (manetteDroite.isValid)
            manetteDroite.TryGetFeatureValue(CommonUsages.trigger, out valeurGachette);
        bool gachetteActuelle = valeurGachette > 0.5f;

        bool vientDEtreAppuye = gachetteActuelle && !gachettePrecedente;
        gachettePrecedente = gachetteActuelle;

        if (survolAvatarActif && vientDEtreAppuye)
            DemarrerSalle();
    }

    void DemarrerSalle()
    {
        salleDemarree = true;

        if (avatar != null)
            avatar.enabled = false;

        if (porte != null)
        {
            porte.Fermer();
            porte.Verrouiller();
        }

        if (effetCouloir != null)
            effetCouloir.DemarrerDisparitionObjets();

        if (timeline != null)
            timeline.Play();

        StartCoroutine(JouerDialogues());

        if (imprimante != null)
            StartCoroutine(LancerImpressionDifferee());

        if (effetCouloir != null)
            StartCoroutine(LancerBattementCoeurDiffere());
    }

    IEnumerator LancerImpressionDifferee()
    {
        yield return new WaitForSeconds(delaiAvantImpression);

        if (bourragePapier)
            imprimante.ImprimerAvecBourrage();
        else
            imprimante.Imprimer();
    }

    IEnumerator LancerBattementCoeurDiffere()
    {
        yield return new WaitForSeconds(delaiAvantBattementCoeur);
        effetCouloir.DemarrerBattementCoeur();
    }

    [ContextMenu("TEST — Valider la salle")]
    void DebugValiderSalle()
    {
        ValiderSalle();
    }

    void TenterValidation()
    {
        if (dialogueTermine)
            ValiderSalle();
    }

    void ValiderSalle()
    {
        if (salleValidee) return;
        salleValidee = true;

        if (porte != null)
        {
            porte.InverserSens();
            porte.Deverrouiller();
        }

        if (porteSuivante != null)
            porteSuivante.Deverrouiller();
    }

    IEnumerator FadeDialogue(float cible)
    {
        if (groupeDialogue == null) yield break;
        float depart = groupeDialogue.alpha;
        float t = 0f;
        while (t < dureeTransition)
        {
            t += Time.deltaTime;
            groupeDialogue.alpha = Mathf.Lerp(depart, cible, t / dureeTransition);
            yield return null;
        }
        groupeDialogue.alpha = cible;
    }

    IEnumerator JouerDialogues()
    {
        yield return StartCoroutine(FadeDialogue(1f));

        if (sourceAudioDialogue != null)
            sourceAudioDialogue.Play();

        if (dialogues != null && dialogues.Length > 0)
        {
            for (int i = 0; i < dialogues.Length; i++)
            {
                if (texteDialogue != null)
                    texteDialogue.text = dialogues[i].texte;

                yield return new WaitForSeconds(dialogues[i].duree);
            }

            if (texteDialogue != null)
                texteDialogue.text = "";
        }
        else if (sourceAudioDialogue != null && sourceAudioDialogue.clip != null)
        {
            yield return new WaitForSeconds(sourceAudioDialogue.clip.length);
        }

        // Sécurité : ne jamais continuer tant que le son du dialogue joue encore réellement
        while (sourceAudioDialogue != null && sourceAudioDialogue.isPlaying)
            yield return null;

        // Sécurité : attendre aussi la fin de la Timeline (l'audio peut être intégré dedans)
        // Note : en mode Hold, state reste Playing après la fin → on vérifie aussi le temps
        while (timeline != null && timeline.state == PlayState.Playing
               && timeline.time + 0.05f < timeline.duration)
            yield return null;

        yield return StartCoroutine(FadeDialogue(0f));

        if (changementLumiere != null)
            changementLumiere.Basculer();

        dialogueTermine = true;
        TenterValidation();
    }
}
