using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ExeEvent
{
    string GetHint();
    void SubmitRun(Vector3 vec);
    void EnterRun();
}

public interface Damage
{
    void Damage(int id, int point, Vector3 vec);
}
