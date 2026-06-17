using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public enum ModeFondu { FonduDebut, FonduFin, FonduDebutEtFin }

    public static FadeManager Instance { get; private set; }

    [Header("Comportement au démarrage")]
    [Tooltip("FonduDebut = noir→transparent au lancement\nFonduFin = transparent→noir à déclencher manuellement\nFonduDebutEtFin = les deux")]
    public ModeFondu mode = ModeFondu.FonduDebutEtFin;

    [Header("Durées par défaut")]
    [Tooltip("Durée du fondu entrant (noir → transparent)")]
    public float dureeEntree = 1.5f;
    [Tooltip("Durée du fondu sortant (transparent → noir)")]
    public float dureeSortie = 1f;
    [Tooltip("Délai avant le fondu d'entrée")]
    public float delaiDepart = 0.2f;

    private Image ecranNoir;
    private Transform canvasTransform;
    private Coroutine coroutineEnCours;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreerEcranNoir();
        SceneManager.sceneLoaded += OnSceneChargee;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChargee;
    }

    // Repositionner le canvas devant la caméra chaque frame — jamais parenté à une caméra de scène
    void Update()
    {
        if (canvasTransform == null || Camera.main == null) return;
        Camera cam = Camera.main;
        canvasTransform.position = cam.transform.position + cam.transform.forward * (cam.nearClipPlane + 0.05f);
        canvasTransform.rotation = cam.transform.rotation;
    }

    void OnSceneChargee(Scene scene, LoadSceneMode modeChargement)
    {
        bool doitFondreEntree = mode == ModeFondu.FonduDebut || mode == ModeFondu.FonduDebutEtFin;
        if (doitFondreEntree && ecranNoir != null)
            ecranNoir.color = Color.black;

        if (doitFondreEntree)
            StartCoroutine(AttendreEtFadeIn());
    }

    IEnumerator AttendreEtFadeIn()
    {
        while (Camera.main == null)
            yield return null;

        FadeIn(dureeEntree);
    }

    // ── API publique ──────────────────────────────────────────

    public void FadeIn(float duree = -1, Action onTermine = null)
    {
        LancerFondu(1f, 0f, duree < 0 ? dureeEntree : duree, onTermine);
    }

    public void FadeOut(float duree = -1, Action onTermine = null)
    {
        LancerFondu(0f, 1f, duree < 0 ? dureeSortie : duree, onTermine);
    }

    public void FadeOutThenIn(Action entreLesDeux, float dureeSortie = -1, float dureeEntree = -1)
    {
        FadeOut(dureeSortie, () =>
        {
            entreLesDeux?.Invoke();
            FadeIn(dureeEntree);
        });
    }

    // ── Interne ───────────────────────────────────────────────

    void CreerEcranNoir()
    {
        var goCanvas = new GameObject("EcranNoir");
        // Parenté au FadeManager (DontDestroyOnLoad) — ne sera jamais détruit par un unload de scène
        goCanvas.transform.SetParent(transform, false);

        var canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 999;

        var rt = goCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(4000f, 4000f);
        rt.localScale = Vector3.one * 0.001f;

        var goImage = new GameObject("Image");
        goImage.transform.SetParent(goCanvas.transform, false);

        ecranNoir = goImage.AddComponent<Image>();
        bool debutNoir = mode == ModeFondu.FonduDebut || mode == ModeFondu.FonduDebutEtFin;
        ecranNoir.color = debutNoir ? Color.black : new Color(0f, 0f, 0f, 0f);

        var imgRt = goImage.GetComponent<RectTransform>();
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.sizeDelta = Vector2.zero;

        canvasTransform = goCanvas.transform;
    }

    void LancerFondu(float alphaDepart, float alphaFin, float duree, Action onTermine)
    {
        if (coroutineEnCours != null)
            StopCoroutine(coroutineEnCours);
        coroutineEnCours = StartCoroutine(AnimerFondu(alphaDepart, alphaFin, duree, onTermine));
    }

    IEnumerator AnimerFondu(float alphaDepart, float alphaFin, float duree, Action onTermine)
    {
        if (alphaDepart == 1f)
            yield return new WaitForSeconds(delaiDepart);

        ecranNoir.color = new Color(0f, 0f, 0f, alphaDepart);

        float t = 0f;
        while (t < duree)
        {
            t += Time.deltaTime;
            ecranNoir.color = new Color(0f, 0f, 0f, Mathf.Lerp(alphaDepart, alphaFin, t / duree));
            yield return null;
        }

        ecranNoir.color = new Color(0f, 0f, 0f, alphaFin);
        onTermine?.Invoke();
    }
}
