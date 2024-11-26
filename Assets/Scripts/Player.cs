using System;
using UnityEngine;

public class Player : MonoBehaviour {
    public event Action<int> OnChipsUpdate;
    public event Action<int> OnBetsChanged;    
    public int ownedChips;
    public int totalBets;

    void Start()
    {
        UpdateChips(ownedChips);
    }

    public void UpdateChips(int newValue)
    {
        ownedChips = newValue;
        OnChipsUpdate?.Invoke(newValue);
    }

    public void AddBets(int value)
    {
        totalBets += value;
        OnBetsChanged.Invoke(totalBets);
    }

    public void AddChips(int value)
    {
        UpdateChips(ownedChips + value);
    }

    public void RemoveChips(int value)
    {
        UpdateChips(ownedChips - value);
    }
}