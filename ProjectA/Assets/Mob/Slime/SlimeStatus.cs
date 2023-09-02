using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStatus : MobStatus
{
    protected override Status InitializeStatus()
    {
        Dictionary<string, int> r = new Dictionary<string, int>();
        r.Add("hp", 10);
        r.Add("attack_range", 50);
        r.Add("detect_range", 100);
        return new Status(r);
    }
}
