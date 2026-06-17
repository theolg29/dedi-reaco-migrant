using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// À poser sur le bouton "Jouer" (Image + Texte, avec Collider + XRSimpleInteractable, comme une porte/imprimante)
[RequireComponent(typeof(XRSimpleInteractable))]
public class MainMenuManager : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Nom exact de la scène à charger en appuyant sur la gâchette pendant le survol du bouton")]
    public string sceneSuivante = "01_Intro";

    private bool partieLancee;

    void Awake()
    {
        var bouton = GetComponent<XRSimpleInteractable>();
        bouton.selectEntered.AddListener(_ => Jouer());
    }

    void Jouer()
    {
        if (partieLancee) return;
        partieLancee = true;

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOutThenIn(() => SceneManager.LoadScene(sceneSuivante));
        else
            SceneManager.LoadScene(sceneSuivante);
    }
}
