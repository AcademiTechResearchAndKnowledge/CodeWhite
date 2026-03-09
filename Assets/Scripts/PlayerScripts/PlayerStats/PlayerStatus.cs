using System;
using UnityEngine;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Dictionary<string, int> playerStat = new Dictionary<string, int>();
    void Start()
    {
        this.playerStat.Add("ANX", 100);
        this.playerStat.Add("STA", 100);
        this.playerStat.Add("SPD", 100);
        this.playerStat.Add("JMP", 100);
    }
    //Setter Functions
    public void addStat(string type, int constant)
    {
        foreach (var Key in this.playerStat)
        {
            if (Key.Key == type)
            {
                this.playerStat[type] += constant;
            }
        }
        ;
    }
    public void subtractStat(string type, int constant)
    {
        foreach (var Key in this.playerStat)
        {
            if (Key.Key == type)
            {
                this.playerStat[type] -= constant;
            }
        }
        ;
    }
    //Getter Function
    public int getStat(string type)
    {
        foreach (var Key in this.playerStat)
        {
            if (Key.Key == type)
            {
                return this.playerStat[type];
            }
        }
        ;
        return -1;
    }
}
