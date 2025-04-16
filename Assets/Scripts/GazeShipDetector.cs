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
    private GameObject currentLookedButton = null;
    private float gazeTimer = 0f;
    private Vector3 gazeTargetPoint;

    public Controls controls;
    private InputAction fireAction;

    private string botonesLayerName = "Botones";

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

    public void ResetDetector()
    {
        // Asegúrate de deshabilitar el control anterior
        if (fireAction != null)
        {
            fireAction.Disable();  // Deshabilitar acción anterior
            fireAction.performed -= _ => OnTriggerPressed(); // Eliminar suscripción
        }

        // Crear nuevos controles y habilitar los eventos
        controls = new Controls();
        fireAction = controls.PlayerControls.Fire;
        fireAction.Enable();
        fireAction.performed += _ => OnTriggerPressed();
    }


    public void EnableControls() => fireAction?.Enable();
    public void DisableControls() => fireAction?.Disable();

    void Update()
    {
        if (EyeManager.Instance == null || !EyeManager.Instance.IsEyeTrackingAvailable()) return;

        Vector3 eyeOrigin, eyeDirection;

        if (EyeManager.Instance.GetCombinedEyeOrigin(out eyeOrigin) &&
            EyeManager.Instance.GetCombindedEyeDirectionNormalized(out eyeDirection))
        {
            Vector3 worldOrigin = Camera.main.transform.TransformPoint(eyeOrigin);
            Vector3 worldDirection = Camera.main.transform.TransformDirection(eyeDirection);
            Ray ray = new Ray(worldOrigin, worldDirection);
            RaycastHit hit;

            int buttonMask = LayerMask.GetMask(botonesLayerName);

            // 1?? Primero miramos si hay botón
            if (Physics.Raycast(ray, out hit, maxDistance, buttonMask))
            {
                GameObject lookedObject = hit.collider.gameObject;
                gazeTargetPoint = hit.point;

                if (lookedObject != currentLookedButton)
                {
                    ResetPreviousLook();
                    currentLookedButton = lookedObject;
                    HighlightButton(currentLookedButton, true);
                }
                return;
            }

            // 2?? Si no hay botón, buscamos barcos
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
                    else if (GameSettings.CurrentShootingMode == GameSettings.DisparoMode.OnlyView ||
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
            currentLookedShip.Highlight(false);
            currentLookedShip = null;
        }

        if (currentLookedButton != null)
        {
            HighlightButton(currentLookedButton, false);
            currentLookedButton = null;
        }

        gazeTimer = 0f;
    }

    void HighlightButton(GameObject button, bool highlight)
    {
        Renderer renderer = button.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = highlight ? new Color(1f, 0.6f, 0f) : Color.white;
            button.transform.localScale = highlight ? button.transform.localScale * 1.1f : button.transform.localScale / 1.1f;
        }
    }

    void OnTriggerPressed()
    {
        if (currentLookedShip != null &&
            (GameSettings.CurrentShootingMode == GameSettings.DisparoMode.OnlyController ||
             GameSettings.CurrentShootingMode == GameSettings.DisparoMode.Both))
        {
            FireCannonball(currentLookedShip);
            ResetPreviousLook();
        }
        else if (currentLookedButton != null)
        {
            // Disparo físico hacia el botón
            GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);
            Rigidbody rb = cannonball.GetComponent<Rigidbody>() ?? cannonball.AddComponent<Rigidbody>();

            Vector3 direction = (gazeTargetPoint - cannonTransform.position).normalized;
            float distance = Vector3.Distance(cannonTransform.position, gazeTargetPoint);
            float adjustedForce = Mathf.Max(200f, distance * forceMultiplier); // Usa una fuerza mínima
            rb.AddForce(direction * adjustedForce);

            ResetPreviousLook();
        }
    }

    void FireCannonball(Ship target)
    {
        GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);
        Rigidbody rb = cannonball.GetComponent<Rigidbody>() ?? cannonball.AddComponent<Rigidbody>();

        Vector3 direction = (gazeTargetPoint - cannonTransform.position).normalized;
        float distance = Vector3.Distance(cannonTransform.position, gazeTargetPoint);
        float adjustedForce = distance * forceMultiplier;
        rb.AddForce(direction * adjustedForce);

        CannonballShip cannonballScript = cannonball.GetComponent<CannonballShip>();
        if (cannonballScript != null)
        {
            cannonballScript.targetShip = target;
        }
    }


}

#endif








