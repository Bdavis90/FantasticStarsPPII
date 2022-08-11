using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManagable
{
    // Start is called before the first frame update
    public void onDeath();
    public void onHit();
}
