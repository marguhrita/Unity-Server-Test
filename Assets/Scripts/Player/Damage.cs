using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Damage : MonoBehaviour
{


   [MessageHandler((ushort)ClientToServerId.damage)]
   private static void dealDamage()
    {

    }
}
