#if WAVE_SDK_IMPORTED

using UnityEngine;
using UnityEngine.InputSystem;
using Wave.Essence.Eye;

public class GazeShipDetector : MonoBehaviour
{
    public float maxDistance = Mathf.Infinity;
    public GameObject cannonballPrefab;
    public Transform cannonTransform;
    public float forceMultiplier = 500f;
    public float gazeHoldTime = 2f;

    private Ship currentLookedShip = null;
    private float gazeTimer = 0f;
    private Vector3 gazeTargetPoint;

    // NUEVO: Input
    public Controls controls;
    private InputAction fireAction;

    void OnEnable()
    {
        controls = new Controls();
        fireAction = controls.PlayerControls.Fire;
        fireAction.Enable();
        fireAction.performed += _ => OnTriggerPressed();
    }

    void OnDisable()
    {
        fireAction.Disable();
    }

    void Update()
    {
        // Verificar si el seguimiento ocular está disponible
        if (EyeManager.Instance == null || !EyeManager.Instance.IsEyeTrackingAvailable())
            return;

        Vector3 eyeOrigin, eyeDirection;

        if (EyeManager.Instance.GetCombinedEyeOrigin(out eyeOrigin) &&
            EyeManager.Instance.GetCombindedEyeDirectionNormalized(out eyeDirection))
        {
            // Convertir el origen y la dirección del ojo a coordenadas del mundo
            Vector3 worldOrigin = Camera.main.transform.TransformPoint(eyeOrigin);
            Vector3 worldDirection = Camera.main.transform.TransformDirection(eyeDirection);

            Ray ray = new Ray(worldOrigin, worldDirection);
            RaycastHit hit;

            // Lanzar un rayo para detectar el barco
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                Ship lookedShip = hit.collider.GetComponentInParent<Ship>();
                gazeTargetPoint = hit.point;

                // Si estamos mirando un barco
                if (lookedShip != null)
                {
                    if (lookedShip != currentLookedShip)
                    {
                        // Si cambiamos de barco, restablecemos la mirada anterior
                        ResetPreviousLook();
                        currentLookedShip = lookedShip;
                        currentLookedShip.Highlight(true);
                        gazeTimer = 0f;
                    }
                    else
                    {
                        // Solo contamos el tiempo de la mirada si estamos en los modos apropiados
                        if (GameSettings.CurrentShootingMode == GameSettings.DisparoMode.OnlyView ||
                            GameSettings.CurrentShootingMode == GameSettings.DisparoMode.Both)
                        {
                            gazeTimer += Time.deltaTime;

                            if (gazeTimer >= gazeHoldTime)
                            {
                                FireCannonball(currentLookedShip);
                                ResetPreviousLook();
                            }
                        }
                    }
                }
                else
                {
                    ResetPreviousLook();
                }
            }
            else
            {
                ResetPreviousLook();
            }
        }
    }

    void ResetPreviousLook()
    {
        if (currentLookedShip != null)
        {
            // Desactivar la iluminación del barco si no estamos mirando a un barco
            currentLookedShip.Highlight(false);
            currentLookedShip = null;
        }

        gazeTimer = 0f;
    }

    // Disparo con gatillo
    void OnTriggerPressed()
    {
        // Solo permitimos disparar si el barco está siendo mirado y el modo es apropiado
        if (currentLookedShip != null)
        {
            // Solo permitimos disparo con gatillo si el modo es SoloMando o Ambas
            if (GameSettings.CurrentShootingMode == GameSettings.DisparoMode.OnlyController ||
                GameSettings.CurrentShootingMode == GameSettings.DisparoMode.Both)
            {
                FireCannonball(currentLookedShip);
                ResetPreviousLook();
            }
        }
    }

    void FireCannonball(Ship target)
    {
        GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();

        if (rb == null)
            rb = cannonball.AddComponent<Rigidbody>();

        // Calculamos la dirección del disparo
        Vector3 direction = (gazeTargetPoint - cannonTransform.position).normalized;
        float distance = Vector3.Distance(cannonTransform.position, gazeTargetPoint);
        float adjustedForce = distance * forceMultiplier;

        // Aplicamos la fuerza al proyectil
        rb.AddForce(direction * adjustedForce);

        CannonballShip cannonballShipScript = cannonball.GetComponent<CannonballShip>();
        if (cannonballShipScript != null)
        {
            cannonballShipScript.targetShip = target;
        }
    }
}

#endif






