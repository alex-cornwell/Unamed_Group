using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveData;
    public int playerHP = 20; // saved between battles and world
    public bool truckToolsGiven = false;
    public bool truckFixed      = false;
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}
