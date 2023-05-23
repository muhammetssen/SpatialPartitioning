using UnityEngine;

class WallManager : MonoBehaviour {
    public GameObject wallPrefab;

    protected virtual void Start(){
        this.wallPrefab = Resources.Load<GameObject>("Wall");
        this.InitiateWalls();
    }
    protected virtual void InitiateWalls(){}
    protected virtual void CreateWall(Vector3 position, float angle){}
    protected virtual void CreateWall(Vector3 position, bool isVertical){}
}
