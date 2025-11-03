using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class UserData
    {
        public string userID;
        public string username;
    
        public InventoryData inventory = new InventoryData();
        public FarmData farm = new FarmData();
        public ProgressionData progression = new ProgressionData();
    
        public DateTime lastSaved = DateTime.Now;
    }
}

[Serializable]
public class InventoryData
{
    public List<string> inventory = new List<string>();
}

[Serializable]
public class FarmData
{
    public List<FarmPlot> plots = new List<FarmPlot>();
}

[Serializable]
public class FarmPlot
{
    public int plotID;
    public string cropType;
    public DateTime plantedTime;
}

[Serializable]
public class ProgressionData
{
    public int level = 1;
    public float experience = 0;
}
