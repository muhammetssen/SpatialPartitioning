using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    [SerializeField]
    public int interval = 1;

    [SerializeField]
    public string identifier = "some object";
    public ClientToServerConnection manager;
    public uint id;
    public bool IsBroadcast = false;
    void FixedUpdate()
    {
        uint expectedServerIndex = Config.GetParcelId(transform.position);
        if(expectedServerIndex != manager.index)
        {
            manager.serverToServerConnection.SendObjectToServer(this, expectedServerIndex);
            manager.alertClientsOfObjectRemoval(id);
            manager.myObjects.Remove(id);
            Destroy(gameObject);
        }
    }
}
