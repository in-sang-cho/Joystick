using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Health, Stamina, Berserk, Weapon}
    public Type type;
    public int value;

    void Update()
    {
        transform.Rotate(Vector3.forward * 20 * Time.deltaTime);
    }
}
