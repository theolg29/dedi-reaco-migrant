using UnityEngine;

public class DevLocomotion : MonoBehaviour
{
    [Header("Mode développement")]
    [Tooltip("Coché : déplacement physique + joystick. Décoché : déplacement physique uniquement.")]
    public bool joystickActif = true;

    [Tooltip("Le GameObject 'Move' de l'XR Origin (contient le DynamicMoveProvider)")]
    public GameObject locomotionJoystick;

    void Awake()
    {
        if (locomotionJoystick == null) return;
        locomotionJoystick.SetActive(joystickActif);
    }
}
