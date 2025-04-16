#if WAVE_SDK_IMPORTED

namespace menu
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.InputSystem;
    using Wave.Essence.Eye;

    public class GazeMenuVive : MonoBehaviour
    {
        [Header("Referencias de los menús")]
        public GameObject MainMenu;
        public GameObject SelectMode;

        [Header("Referencias de los 'botones'")]
        public GameObject TestMode;
        public GameObject RandomMode;
        public GameObject Exit;

        public GameObject OnlyView;
        public GameObject OnlyController;
        public GameObject Both;
        public GameObject Back;

        [Header("Distancia de interacción")]
        public float maxDistance = Mathf.Infinity;

        [Header("Configuración de input")]
        public Controls controls;
        private InputAction fireAction;

        private GameObject currentLookedObject = null;
        private Color originalColor;
        private Material currentMat;

        private string botonesLayerName = "Botones";

        [Header("Prefab de la bola de cañón y Transform del cañón")]
        public GameObject cannonballPrefab;
        public Transform cannonTransform;
        public float forceMultiplier = 500f; // Ajustar fuerza

        private Vector3 gazeTargetPoint; // Punto donde irá la bala

        private GameObject modoTestObject;
        private GameObject modoAleatorioObject;
        private GameObject salirObject;

        private GameObject onlyViewObject;
        private GameObject onlyControllerObject;
        private GameObject bothObject;
        private GameObject backObject;

        void Start()
        {
            MainMenu.SetActive(true);
            SelectMode.SetActive(false);

            modoTestObject = TestMode;
            modoAleatorioObject = RandomMode;
            salirObject = Exit;
            onlyViewObject = OnlyView;
            onlyControllerObject = OnlyController;
            bothObject = Both;
            backObject = Back;
        }

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

                int mask = LayerMask.GetMask(botonesLayerName);

                if (Physics.Raycast(ray, out hit, maxDistance, mask))
                {
                    GameObject lookedObject = hit.collider.gameObject;
                    gazeTargetPoint = hit.point;

                    if (lookedObject != currentLookedObject)
                    {
                        ResetPreviousLook();
                        HighlightObject(lookedObject);
                    }
                }
                else if (currentLookedObject != null)
                {
                    ResetPreviousLook();
                }
            }
        }

        void HighlightObject(GameObject obj)
        {
            currentLookedObject = obj;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                currentMat = renderer.material;
                originalColor = currentMat.color;
                currentMat.color = new Color(1f, 0.6f, 0f);
                obj.transform.localScale *= 1.1f;
            }
        }

        void ResetPreviousLook()
        {
            if (currentLookedObject != null)
            {
                Renderer renderer = currentLookedObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = originalColor;
                    currentLookedObject.transform.localScale /= 1.1f;
                }
                currentLookedObject = null;
            }
        }

        void OnTriggerPressed()
        {
            if (currentLookedObject == null) return;

            GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);

            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = cannonball.AddComponent<Rigidbody>();
            }

            Vector3 direction = (gazeTargetPoint - cannonTransform.position).normalized;
            float distance = Vector3.Distance(cannonTransform.position, gazeTargetPoint);
            float adjustedForce = distance * forceMultiplier;

            rb.AddForce(direction * adjustedForce);

            Cannonball cannonballScript = cannonball.AddComponent<Cannonball>();
            cannonballScript.targetButton = currentLookedObject;
            cannonballScript.menuController = this;
        }

        public void ExecuteButtonAction(GameObject button)
        {
            if (button == modoTestObject)
            {
                MainMenu.SetActive(false);
                SelectMode.SetActive(true);
            }
            else if (button == modoAleatorioObject)
            {
                Debug.Log("Modo aleatorio todavía no implementado.");
            }
            else if (button == salirObject)
            {
                Application.Quit();
            }
            else if (button == onlyViewObject)
            {
                GameSettings.CurrentShootingMode = GameSettings.DisparoMode.OnlyView;
                SceneManager.LoadScene("ModoTest");
            }
            else if (button == onlyControllerObject)
            {
                GameSettings.CurrentShootingMode = GameSettings.DisparoMode.OnlyController;
                SceneManager.LoadScene("ModoTest");
            }
            else if (button == bothObject)
            {
                GameSettings.CurrentShootingMode = GameSettings.DisparoMode.Both;
                SceneManager.LoadScene("ModoTest");
            }
            else if (button == backObject)
            {
                SelectMode.SetActive(false);
                MainMenu.SetActive(true);
            }
        }
    }
}

#endif









