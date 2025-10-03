using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class CustomVolumeComponent : VolumeComponent
{
    public ClampedFloatParameter horizontalBlur =
        new ClampedFloatParameter(0.05f, 0, 0.5f);
    public ClampedFloatParameter verticalBlur =
        new ClampedFloatParameter(0.05f, 0, 0.5f);
}


// [Serializable]
// public class OutlineVolumeComponent : VolumeComponent
// {
//     public ClampedFloatParameter thickness = new ClampedFloatParameter(0.05f, 0, 0.5f);
//     public ClampedFloatParameter depthSens = new ClampedFloatParameter(0.05f, 0, 0.5f);
//     public ClampedFloatParameter normalSens = new ClampedFloatParameter(0.05f, 0, 0.5f);
//     public ColorParameter color = new ColorParameter(Color.black);
// }



