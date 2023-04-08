using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectedTo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateText());
        transform.position = new Vector3(0, 0,-Config.ParcelSize / 2);
    }

    private IEnumerator UpdateText()
    {
        while (true)
        {
            this.gameObject.GetComponent<TMP_Text>().text = "Connected to: " + Config.GetClient2ServerPort(ClientConnection.serverIndex);

            yield return new WaitForSeconds(1);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
