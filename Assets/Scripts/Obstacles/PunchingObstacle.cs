using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingObstacle : MonoBehaviour
{
    [SerializeField] private ParticleSystem punchVfx;

    public void SimulateHit(Collider hit)
    {
        punchVfx.Play();
    }

}
