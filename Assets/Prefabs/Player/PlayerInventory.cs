using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // 使用 HashSet 存储钥匙颜色，防止重复且查询快
    private HashSet<string> collectedKeys = new HashSet<string>();

    // 捡到钥匙时调用
    public void AddKey(string keyColor)
    {
        if (!collectedKeys.Contains(keyColor))
        {
            collectedKeys.Add(keyColor);
            Debug.Log($"捡到了 {keyColor} 钥匙！");
        }
    }

    // 门调用这个函数检查玩家是否有对应钥匙
    public bool HasKey(string keyColor)
    {
        return collectedKeys.Contains(keyColor);
    }
}