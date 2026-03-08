using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // 使用字典来记录：<钥匙颜色, 拥有数量>
    private Dictionary<string, int> keyCounts = new Dictionary<string, int>();

    // 拾取钥匙时调用（Key.cs 中的逻辑保持不变即可调用这个）
    public void AddKey(string colorID)
    {
        if (keyCounts.ContainsKey(colorID))
        {
            keyCounts[colorID]++; // 数量 +1
        }
        else
        {
            keyCounts[colorID] = 1; // 第一次获得该颜色，设为 1
        }
        Debug.Log($"获得了 {colorID} 钥匙，当前该颜色钥匙数量: {keyCounts[colorID]}");
    }

    // 检查是否有该颜色的钥匙，并且数量大于 0
    public bool HasKey(string colorID)
    {
        return keyCounts.ContainsKey(colorID) && keyCounts[colorID] > 0;
    }

    // 消耗钥匙（开门时调用）
    public void UseKey(string colorID)
    {
        if (HasKey(colorID))
        {
            keyCounts[colorID]--; // 数量 -1
            Debug.Log($"消耗了 {colorID} 钥匙，剩余数量: {keyCounts[colorID]}");
        }
    }
}