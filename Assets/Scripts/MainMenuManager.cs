using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

// À poser sur le bouton "Jouer" (Image + Texte), parenté à la Main Camera pour qu'il suive toujours le regard
public class MainMenuManager : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Nom exact de la scène à charger en appuyant sur la gâchette (pas besoin de viser)")]
    public string sceneSuivante = "01_Intro";

    private InputDevice manetteDroite;
    private bool gachettePrecedente;
    private bool partieLancee;

    void Update()
    {
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

        if (vientDEtreAppuye)
            Jouer();
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
