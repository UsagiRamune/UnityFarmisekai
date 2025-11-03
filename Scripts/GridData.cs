using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    public  Dictionary<Vector3Int, PlacementData> placeObjects = new Dictionary<Vector3Int, PlacementData>();

    public void AddObjectAt(Vector3Int pos, Vector2Int objectSize, int ID, int placedObjectIndex, int rotateIndex,DirtTile dirtTile = null)
    {
        List<Vector3Int> positionToOccupy = CalculateRotatedPos(pos, objectSize, rotateIndex);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);
        foreach (var position in positionToOccupy)
        {
            if (placeObjects.ContainsKey(position))
                throw new Exception($"Duplicate placement object at {position}");
            placeObjects[position] = data;
        }
        if(dirtTile != null)
        data.DirtTile =  dirtTile;
    }
    public List<Vector3Int> CalculateRotatedPos(Vector3Int origin, Vector2Int size, int rotateIndex)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        int width = size.x;
        int height = size.y;

        // Swap width/height for 90° or 270° rotation
        if (rotateIndex % 2 == 1)
        {
            int tmp = width;
            width = height;
            height = tmp;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int offset;
                switch (rotateIndex % 4)
                {
                    case 0: offset = new Vector3Int(x, 0, y); break;
                    case 1: offset = new Vector3Int(x, 0, -y); break;
                    case 2: offset = new Vector3Int(-x,0,-y); break;
                    case 3: offset = new Vector3Int(-x,0,y); break;
                    default: offset = new Vector3Int(x, 0, y); break;
                }
                positions.Add(origin + offset);
            }
        }
        return positions;
    }
   

    public bool CanPlaceObject(Vector3Int pos, Vector2Int objectSize, int rotateIndex,Vector3Int playerGridPos)
    {
        List<Vector3Int> positionToOccupy = CalculateRotatedPos(pos, objectSize, rotateIndex);

        int halfSize = (PlacementSystem.instance.farmsize - 1) / 2;
        int farmMin = -halfSize;
        int farmMax = halfSize;

        foreach (var position in positionToOccupy)
        {
            // 1. Already occupied
            if (placeObjects.ContainsKey(position))
                return false;

            // 2. Player position
            if (position == playerGridPos)
                return false;

            // 3. Outside farm bounds
            if (position.x < farmMin || position.x > farmMax || position.z < farmMin || position.z > farmMax)
                return false;
        }

        return true;
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPos;
    public int ID { get; private set; }
    public int PlaceObjectIndex { get; private set; }
    public GameObject PlantGO;
    public PlacementData(List<Vector3Int> occupiedPos, int iD, int placeObjectIndex)
    {
        this.occupiedPos = occupiedPos;
        ID = iD;
        PlaceObjectIndex = placeObjectIndex;
    }
    public PlantDataSO.PlantData PlantData;
    public DirtTile DirtTile;
    public PlantGrowing PlantRef;
    public bool HasPlant => PlantGO != null;

    public void SetPlant(GameObject plantGO, PlantDataSO.PlantData data)
    {
        PlantGO = plantGO;
        PlantData = data;
    }
}
