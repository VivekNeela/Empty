using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tmkoc.physicsAdv
{
    public class RoofCheck : MonoBehaviour
    {
        public bool isRoofed = false;

        void OnCollisionEnter2D(Collision2D collision)
        {
            isRoofed = true;
        }
    }
}
