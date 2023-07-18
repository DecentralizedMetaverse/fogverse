using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPipe 
{
    public void Receive(params object[] args);    
}
