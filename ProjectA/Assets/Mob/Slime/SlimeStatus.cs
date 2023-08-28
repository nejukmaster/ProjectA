using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStatus : MobStatusManager
{
    protected override Status InitializeStatus()
    {
        Dictionary<string, float> r = new Dictionary<string, float>();
        r.Add("hp", 10f);
        return new Status(r);
    }
}
