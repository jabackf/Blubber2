using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestSignCallback : MonoBehaviour
{
    public void TestCallbackOne(Dialog d)
    {
        int a;
        if ( Int32.TryParse(d.getAnswer(), out a) )
        {
            a += 500;
            d.setNextText(d.getNextText() + " More like " + a.ToString() + " years old!");
        }
    }
}
