using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class DetectLight : MonoBehaviour
{
    private GameObject lightSource;
    private GameObject playerInstance;
    [SerializeField] private bool setTimer = true;
    [SerializeField] private float timer = 0;

    private void Awake()
    {
        lightSource = GameObject.Find("LightSource");
        if (lightSource == null)
        {
            Debug.Log("There is no SpotLight Object that was found in the tree.");
        }
        playerInstance = GameObject.Find("Player");
        if (lightSource == null)
        {
            Debug.Log("Player Object not found in tree.");
        }
        GameObject collider = lightSource.transform.GetChild(0).gameObject;
    }
    //Premade functions
    private void OnTriggerEnter(Collider other)
    {
        //Despawn Liminal Shade Instance Here
        Debug.Log("The Liminal Shade has unspawned.");
        setTimer = false;
    }
    private void OnTriggerExit(Collider other)
    {
        setTimer = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (setTimer && timer <= 10f)
        {
            timer += Time.deltaTime;
            if (timer >= 10f)
            {
                //Insert Liminal Shade Instance Here
                Debug.Log("The Liminal Shade has spawned.");
            }
        } else if (!setTimer)
        {
            timer = 0;
        }
    }
}
