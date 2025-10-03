using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class OutlineVolumeComponent : VolumeComponent
{
    public ClampedFloatParameter thickness = new ClampedFloatParameter(0.05f, 0, 0.5f);
    public ClampedFloatParameter depthSens = new ClampedFloatParameter(0.05f, 0, 0.5f);
    public ClampedFloatParameter normalSens = new ClampedFloatParameter(0.05f, 0, 0.5f);
    public ColorParameter color = new ColorParameter(Color.black);
}
