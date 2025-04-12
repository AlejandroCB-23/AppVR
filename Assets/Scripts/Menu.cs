#if WAVE_SDK_IMPORTED

namespace menu
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.InputSystem;
    using Wave.Essence.Eye;

    public class GazeMenuVive : MonoBehaviour
    {
        [Header("Referencias de los 'botones'")]
        private GameObject modoTestObject;
        private GameObject modoAleatorioObject;
        private GameObject salirObject;

        [Header("Distancia de interacción")]
        public float maxDistance = Mathf.Infinity;

        [Header("Configuración de input")]
        public Controls controls;  // Tu Input Actions
        private InputAction fireAction;

        private GameObject currentLookedObject = null;
        private Color originalColor;
        private Material currentMat;

        private string botonesLayerName = "Botones";

        [Header("Prefab de la bola de cañón y Transform del cañón")]
        public GameObject cannonballPrefab;
        public Transform cannonTransform;
        public float cannonballForce = 1000f;  // Ajusta la fuerza según lo necesites

        // Se añade Start para inicializar las referencias de los objetos
        void Start()
        {
            // Buscar los objetos en la escena por nombre
            modoTestObject = GameObject.Find("ModoTest");
            modoAleatorioObject = GameObject.Find("ModoAleatorio");
            salirObject = GameObject.Find("Salir");

            // Asegurarse de que se han encontrado correctamente
            if (modoTestObject == null || modoAleatorioObject == null || salirObject == null)
            {
                Debug.LogError("No se han encontrado todos los objetos. Asegúrate de que los objetos existen en la escena y tienen los nombres correctos.");
            }
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

                    // Si se detecta un nuevo objeto o si el objeto ha cambiado
                    if (lookedObject != currentLookedObject)
                    {
                        ResetPreviousLook(); // Restablecer el anterior
                        HighlightObject(lookedObject); // Resaltar el nuevo
                    }

                    // Asegurarse de que el botón sigue siendo el que estás mirando
                    if (lookedObject != currentLookedObject && currentLookedObject != null)
                    {
                        ResetPreviousLook(); // Si se deja de mirar el objeto, se resetea
                    }
                }
                else if (currentLookedObject != null)
                {
                    // Si no se está mirando ningún objeto, resetear el estado
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
                currentMat.color = new Color(1f, 0.6f, 0f);  // Naranja dorado para el resalto
                obj.transform.localScale *= 1.1f;  // Aumentar el tamaño ligeramente
            }
        }

        void ResetPreviousLook()
        {
            if (currentLookedObject != null)
            {
                Renderer renderer = currentLookedObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = originalColor;  // Restablecer color original
                    currentLookedObject.transform.localScale /= 1.1f;  // Restablecer escala original
                }
                currentLookedObject = null;  // Restablecer el objeto actualmente seleccionado
            }
        }

        void OnTriggerPressed()
        {
            if (currentLookedObject == null) return;

            // Instanciar la bola de cañón desde la posición del cañón
            GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);

            // Asegurarse de que la bola tenga Rigidbody (si no, se le añade)
            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = cannonball.AddComponent<Rigidbody>();
            }

            // Calcular la dirección desde el cañón hacia el botón
            Vector3 direction = (currentLookedObject.transform.position - cannonTransform.position).normalized;
            rb.AddForce(direction * cannonballForce);

            // Añadir el script que se encargará de detectar la colisión y ejecutar la acción
            Cannonball cannonballScript = cannonball.AddComponent<Cannonball>();
            cannonballScript.targetButton = currentLookedObject;
            cannonballScript.menuController = this;
        }

        // Este método se llama desde la bola de cañón cuando impacta con el botón
        public void ExecuteButtonAction(GameObject button)
        {
            if (button == modoTestObject)
            {
                SceneManager.LoadScene("Game");
            }
            else if (button == modoAleatorioObject)
            {
                Debug.Log("Modo aleatorio todavía no implementado.");
            }
            else if (button == salirObject)
            {
                Application.Quit();
            }
        }
    }
}

#endif







