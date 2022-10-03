using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public abstract class UtilityItem : DefaultItem
{
    public abstract void UseUtility(Vector3 aimDirection);
    public abstract void CancelUtility();
}
