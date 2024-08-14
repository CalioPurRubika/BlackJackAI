using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Card 
{
    public int cardValue;
    public string cardName;

    public Card(int value, string name)
    {
        cardValue = value;
        cardName = name;    
    }
}
