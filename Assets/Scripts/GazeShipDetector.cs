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
        if (EyeManager.Instance == null || !EyeManager.Instance.IsEyeTrackingAvailable())
            return;

        Vector3 eyeOrigin, eyeDirection;

        if (EyeManager.Instance.GetCombinedEyeOrigin(out eyeOrigin) &&
            EyeManager.Instance.GetCombindedEyeDirectionNormalized(out eyeDirection))
        {
            Vector3 worldOrigin = Camera.main.transform.TransformPoint(eyeOrigin);
            Vector3 worldDirection = Camera.main.transform.TransformDirection(eyeDirection);

            Ray ray = new Ray(worldOrigin, worldDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                Ship lookedShip = hit.collider.GetComponentInParent<Ship>();
                gazeTargetPoint = hit.point;

                if (lookedShip != null)
                {
                    if (lookedShip != currentLookedShip)
                    {
                        ResetPreviousLook();
                        currentLookedShip = lookedShip;
                        currentLookedShip.Highlight(true);
                        gazeTimer = 0f;
                    }
                    else
                    {
                        gazeTimer += Time.deltaTime;

                        if (gazeTimer >= gazeHoldTime)
                        {
                            FireCannonball(currentLookedShip);
                            ResetPreviousLook();
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
            // Desactivar el círculo indicador si no estamos mirando el barco
            currentLookedShip.Highlight(false);
            currentLookedShip = null;
        }

        gazeTimer = 0f;
    }

    // NUEVO: Disparo con gatillo
    void OnTriggerPressed()
    {
        if (currentLookedShip != null)
        {
            FireCannonball(currentLookedShip);
            ResetPreviousLook();
        }
    }

    void FireCannonball(Ship target)
    {
        GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();

        if (rb == null)
            rb = cannonball.AddComponent<Rigidbody>();

        Vector3 direction = (gazeTargetPoint - cannonTransform.position).normalized;
        float distance = Vector3.Distance(cannonTransform.position, gazeTargetPoint);
        float adjustedForce = distance * forceMultiplier;

        rb.AddForce(direction * adjustedForce);

        CannonballShip cannonballShipScript = cannonball.GetComponent<CannonballShip>();
        if (cannonballShipScript != null)
        {
            cannonballShipScript.targetShip = target;
        }
    }
}

#endif





