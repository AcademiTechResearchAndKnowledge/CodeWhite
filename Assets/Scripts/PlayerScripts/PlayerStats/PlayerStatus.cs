using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerStatus : MonoBehaviour
{
    private Dictionary<string, float> playerStat = new Dictionary<string, float>
    {
        {"ANX", 100f}, {"STA", 100f}, {"SPD", 5f}, {"JMP", 5f}
    };

    void Update() 
    {
        
    }
    //Setter Functions
    public void addStat(string type, float constant)
    {
        foreach (var Key in playerStat)
        {
            if (Key.Key == type)
            {
                this.playerStat[type] += constant;
            }
        }
        ;
    }
    public void subtractStat(string type, float constant)
    {
        foreach (var Key in playerStat)
        {
            if (Key.Key == type)
            {
                this.playerStat[type] -= constant;
            }
        }
        ;
    }
    //Getter Function
    public float getStat(string type)
    {
        foreach (var Key in playerStat)
        {
            if (Key.Key == type)
            {
                return playerStat[type];
            }
        }
        ;
        return -1;
    }
}
