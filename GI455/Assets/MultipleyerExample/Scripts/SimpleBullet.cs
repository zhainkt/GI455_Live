using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var netObj = GetComponent<NetObject>();

        netObj.Destroy(2.0f);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += Vector3.up * 10 * Time.deltaTime;
    }
}
