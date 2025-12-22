using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    private readonly Dictionary<ItemMaster, int> _consts = new();
    public event Action<ItemMaster, int> OnItemCountChanged;

    // シングルトンの初期化
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    // アイテムの所持数を取得する
    public int GetItemCount(ItemMaster item)
    {
        if (_consts.TryGetValue(item, out int count))
        {
            return count;
        }
        return 0;
    }
    // アイテムを追加する
    public void AddItem(ItemMaster item, int count = 1)
    {
        if (_consts.ContainsKey(item))
        {
            _consts[item] += count;
        }
        else
        {
            _consts[item] = count;
        }
        OnItemCountChanged?.Invoke(item, _consts[item]);
    }
    // アイテムを消費する。成功したらtrue、在庫不足で失敗したらfalseを返す。
    public bool TryConsumeItem(ItemMaster item, int count = 1)
    {
        if (GetItemCount(item) >= count)
        {
            _consts[item] -= count;
            OnItemCountChanged?.Invoke(item, _consts[item]);
            return true;
        }
        return false;
    }
    // アイテムを完全に削除する
    public void RemoveItem(ItemMaster item) {
        if (_consts.ContainsKey(item)) {
            _consts.Remove(item);
            OnItemCountChanged?.Invoke(item, 0);
        }
    }
}
