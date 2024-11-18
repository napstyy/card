using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public event Action<int> OnChipsUpdate;
    public int ownedChips;

    void Start()
    {
        UpdateChips(ownedChips);
    }

    public void UpdateChips(int newValue)
    {
        ownedChips= newValue;
        OnChipsUpdate?.Invoke(newValue);
    }
}
