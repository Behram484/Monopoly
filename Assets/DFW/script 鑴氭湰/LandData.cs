using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class LandInfo
{
    public int position;
    public string landName;
    public string group;
    public string action;
    public bool canBePurchased;
    public int purchasePrice;
    public int toll;
    
    // 添加房屋级别对应的租金
    public int rent1House;
    public int rent2Houses;
    public int rent3Houses;
    public int rent4Houses;
    public int rentHotel;
    
    // 现有属性
    public LandType landType;
    public int maxLevel = 5; // 设置为5，对应1-4房+酒店
    public int upgradeCost;
    public string eventMessage;
    // 其他需要的属性...
}

[CreateAssetMenu(fileName = "LandData", menuName = "游戏数据/地块数据")]
public class LandData : ScriptableObject
{
    public List<LandInfo> lands = new List<LandInfo>();
}
