using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Serializable]
    public class Diapositive
    {
        [Tooltip("Image à afficher (laisser vide = placeholder gris)")]
        public Sprite image;
        [Tooltip("Son joué pendant cette diapositive (optionnel)")]
        public AudioClip son;
        [Tooltip("Durée d'affichage en secondes")]
        public float duree = 4f;
    }

    [Header("Diapositives")]
    public Diapositive[] diapositives;

    [Header("Navigation")]
    [Tooltip("Nom exact de la scène à charger après l'intro")]
    public string sceneSuivante = "02_Game";

    [Header("Écran")]
    [Tooltip("Distance de l'écran devant la caméra (mètres)")]
    public float distanceEcran = 3f;
    [Tooltip("Hauteur de l'écran en mètres (largeur calculée en 16/9)")]
    public float hauteurEcran = 2f;

    [Header("Transitions")]
    [Tooltip("Durée du fondu entre chaque diapositive")]
    public float dureeTransition = 0.6f;

    private Image imageAffichage;
    private AudioSource sourceAudio;

    void Start()
    {
        CreerEcran();
        StartCoroutine(LancerIntro());
    }

    void CreerEcran()
    {
        Camera cam = Camera.main;

        var goCanvas = new GameObject("EcranIntro");
        goCanvas.transform.SetParent(cam.transform, false);

        var canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        float largeur = hauteurEcran * (16f / 9f);
        var rt = goCanvas.GetComponent<RectTransform>();
        rt.localPosition = new Vector3(0f, 0f, distanceEcran);
        rt.localRotation = Quaternion.identity;
        rt.sizeDelta = new Vector2(largeur * 100f, hauteurEcran * 100f);
        rt.localScale = Vector3.one * 0.01f;

        // Image de la diapositive
        var goImage = new GameObject("Image");
        goImage.transform.SetParent(goCanvas.transform, false);
        imageAffichage = goImage.AddComponent<Image>();
        imageAffichage.color = new Color(1f, 1f, 1f, 0f);

        var imgRt = goImage.GetComponent<RectTransform>();
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.sizeDelta = Vector2.zero;

        // AudioSource 2D pour le son des diapositives
        sourceAudio = goCanvas.AddComponent<AudioSource>();
        sourceAudio.spatialBlend = 0f;
        sourceAudio.playOnAwake = false;
    }

    IEnumerator LancerIntro()
    {
        // Attendre la fin du FadeIn si FadeManager est présent
        if (FadeManager.Instance != null)
            yield return new WaitForSeconds(FadeManager.Instance.delaiDepart + FadeManager.Instance.dureeEntree);

        foreach (var diapo in diapositives)
        {
            // Appliquer l'image (placeholder gris si aucune)
            if (diapo.image != null)
            {
                imageAffichage.sprite = diapo.image;
                imageAffichage.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                imageAffichage.sprite = null;
                imageAffichage.color = new Color(0.3f, 0.3f, 0.3f, 0f);
            }

            // Jouer le son
            if (diapo.son != null)
            {
                sourceAudio.clip = diapo.son;
                sourceAudio.Play();
            }

            // Fondu entrant
            yield return StartCoroutine(FondreImage(0f, 1f));

            // Affichage
            yield return new WaitForSeconds(diapo.duree);

            // Fondu sortant
            yield return StartCoroutine(FondreImage(1f, 0f));

            sourceAudio.Stop();
        }

        // Charger la scène suivante
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOut(onTermine: () => SceneManager.LoadScene(sceneSuivante));
        else
            SceneManager.LoadScene(sceneSuivante);
    }

    IEnumerator FondreImage(float alphaDepart, float alphaFin)
    {
        float t = 0f;
        Color c = imageAffichage.color;
        c.a = alphaDepart;
        while (t < dureeTransition)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(alphaDepart, alphaFin, t / dureeTransition);
            imageAffichage.color = c;
            yield return null;
        }
        c.a = alphaFin;
        imageAffichage.color = c;
    }
}
