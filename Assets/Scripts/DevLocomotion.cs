using UnityEngine;

public class DevLocomotion : MonoBehaviour
{
    [Header("Mode développement")]
    [Tooltip("Coché : déplacement physique + joystick. Décoché : déplacement physique uniquement.")]
    public bool joystickActif = true;

    [Tooltip("Le GameObject 'Move' de l'XR Origin (contient le DynamicMoveProvider)")]
    public GameObject locomotionJoystick;

    [Tooltip("Le GameObject 'Turn' de l'XR Origin (contient le SnapTurnProvider et le ContinuousTurnProvider)")]
    public GameObject locomotionRotationJoystick;

    void Awake()
    {
        if (locomotionJoystick != null)
            locomotionJoystick.SetActive(joystickActif);

        if (locomotionRotationJoystick != null)
            locomotionRotationJoystick.SetActive(joystickActif);
    }
}
