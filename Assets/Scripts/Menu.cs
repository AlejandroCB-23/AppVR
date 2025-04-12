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

        [Header("Distancia de interacci�n")]
        public float maxDistance = Mathf.Infinity;

        [Header("Configuraci�n de input")]
        public Controls controls;  // Tu Input Actions
        private InputAction fireAction;

        private GameObject currentLookedObject = null;
        private Color originalColor;
        private Material currentMat;

        private string botonesLayerName = "Botones";

        [Header("Prefab de la bola de ca��n y Transform del ca��n")]
        public GameObject cannonballPrefab;
        public Transform cannonTransform;
        public float cannonballForce = 1000f;  // Ajusta la fuerza seg�n lo necesites

        // Se a�ade Start para inicializar las referencias de los objetos
        void Start()
        {
            // Buscar los objetos en la escena por nombre
            modoTestObject = GameObject.Find("ModoTest");
            modoAleatorioObject = GameObject.Find("ModoAleatorio");
            salirObject = GameObject.Find("Salir");

            // Asegurarse de que se han encontrado correctamente
            if (modoTestObject == null || modoAleatorioObject == null || salirObject == null)
            {
                Debug.LogError("No se han encontrado todos los objetos. Aseg�rate de que los objetos existen en la escena y tienen los nombres correctos.");
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

                    // Asegurarse de que el bot�n sigue siendo el que est�s mirando
                    if (lookedObject != currentLookedObject && currentLookedObject != null)
                    {
                        ResetPreviousLook(); // Si se deja de mirar el objeto, se resetea
                    }
                }
                else if (currentLookedObject != null)
                {
                    // Si no se est� mirando ning�n objeto, resetear el estado
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
                obj.transform.localScale *= 1.1f;  // Aumentar el tama�o ligeramente
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

            // Instanciar la bola de ca��n desde la posici�n del ca��n
            GameObject cannonball = Instantiate(cannonballPrefab, cannonTransform.position, Quaternion.identity);

            // Asegurarse de que la bola tenga Rigidbody (si no, se le a�ade)
            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = cannonball.AddComponent<Rigidbody>();
            }

            // Calcular la direcci�n desde el ca��n hacia el bot�n
            Vector3 direction = (currentLookedObject.transform.position - cannonTransform.position).normalized;
            rb.AddForce(direction * cannonballForce);

            // A�adir el script que se encargar� de detectar la colisi�n y ejecutar la acci�n
            Cannonball cannonballScript = cannonball.AddComponent<Cannonball>();
            cannonballScript.targetButton = currentLookedObject;
            cannonballScript.menuController = this;
        }

        // Este m�todo se llama desde la bola de ca��n cuando impacta con el bot�n
        public void ExecuteButtonAction(GameObject button)
        {
            if (button == modoTestObject)
            {
                SceneManager.LoadScene("Game");
            }
            else if (button == modoAleatorioObject)
            {
                Debug.Log("Modo aleatorio todav�a no implementado.");
            }
            else if (button == salirObject)
            {
                Application.Quit();
            }
        }
    }
}

#endif







