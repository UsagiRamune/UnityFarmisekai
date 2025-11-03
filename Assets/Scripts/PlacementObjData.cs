using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlacementObjData : ScriptableObject
{
    public List<PlacementObj> placementDatas;
  

}
[Serializable]
public class PlacementObj
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField] public Vector2Int Size { get; private set; } = Vector2Int.one;
    public float buildTime = 1f;
    [field: SerializeField]
    public GameObject Prefab{ get; private set; }
}
