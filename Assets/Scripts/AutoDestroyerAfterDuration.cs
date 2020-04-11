using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyerAfterDuration : MonoBehaviour
{
    public float Duration = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyAfterDuration", Duration);
    }

    // Update is called once per frame
    private void DestroyAfterDuration()
    {
        Destroy(gameObject);
    }
}
