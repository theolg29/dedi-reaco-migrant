using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Vidéo")]
    [Tooltip("Vidéo d'intro, son déjà intégré dans le fichier")]
    public VideoClip video;

    [Header("Navigation")]
    [Tooltip("Nom exact de la scène à charger après l'intro")]
    public string sceneSuivante = "02_Game";

    [Header("Écran")]
    [Tooltip("Distance de l'écran devant la caméra (mètres)")]
    public float distanceEcran = 3f;
    [Tooltip("Hauteur de l'écran en mètres (largeur calculée en 16/9)")]
    public float hauteurEcran = 2f;

    private VideoPlayer lecteurVideo;

    void Start()
    {
        StartCoroutine(LancerIntro());
    }

    void CreerEcran()
    {
        Camera cam = Camera.main;

        var goCanvas = new GameObject("EcranIntro");

        var canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        float largeur = hauteurEcran * (16f / 9f);
        var rt = goCanvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(largeur * 100f, hauteurEcran * 100f);
        rt.localScale = Vector3.one * 0.01f;

        // Pas de parent caméra : position fixée une seule fois dans l'espace du monde.
        // Suivre la tête en rigide (comme avant) donne le mal des transports en VR.
        rt.position = cam.transform.position + cam.transform.forward * distanceEcran;
        rt.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

        var goImage = new GameObject("VideoImage");
        goImage.transform.SetParent(goCanvas.transform, false);
        var imageVideo = goImage.AddComponent<RawImage>();

        var imgRt = goImage.GetComponent<RectTransform>();
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.sizeDelta = Vector2.zero;

        var rendu = new RenderTexture(1920, 1080, 0);
        imageVideo.texture = rendu;

        var sourceAudio = goCanvas.AddComponent<AudioSource>();
        sourceAudio.spatialBlend = 0f;

        lecteurVideo = goCanvas.AddComponent<VideoPlayer>();
        lecteurVideo.playOnAwake = false;
        lecteurVideo.source = VideoSource.VideoClip;
        lecteurVideo.clip = video;
        lecteurVideo.renderMode = VideoRenderMode.RenderTexture;
        lecteurVideo.targetTexture = rendu;
        lecteurVideo.audioOutputMode = VideoAudioOutputMode.AudioSource;
        lecteurVideo.SetTargetAudioSource(0, sourceAudio);
        lecteurVideo.loopPointReached += _ => TerminerIntro();
    }

    IEnumerator LancerIntro()
    {
        // Attendre la fin du FadeIn si FadeManager est présent — laisse aussi le temps
        // à la pose de la caméra XR de se stabiliser avant de positionner l'écran (CreerEcran).
        if (FadeManager.Instance != null)
            yield return new WaitForSeconds(FadeManager.Instance.delaiDepart + FadeManager.Instance.dureeEntree);

        CreerEcran();
        lecteurVideo.Play();
    }

    void TerminerIntro()
    {
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOut(onTermine: () => SceneManager.LoadScene(sceneSuivante));
        else
            SceneManager.LoadScene(sceneSuivante);
    }
}
