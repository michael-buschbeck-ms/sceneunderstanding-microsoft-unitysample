using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using UnityEngine.XR.WSA;
using System.Runtime.InteropServices;
using Windows.Perception.Spatial;
using Windows.Perception.Spatial.Preview;
#endif

public class LowLevelAnchor : MonoBehaviour
{
#if WINDOWS_UWP
    /// <summary>
    /// SpatialCoordinateSystem used by Unity as the entire scene's frame of reference.
    /// </summary>
    private SpatialCoordinateSystem unitySpatialCoordinateSystem;

    /// <summary>
    /// SpatialCoordinateSystem of the low-level anchor.
    /// </summary>
    private SpatialCoordinateSystem anchorSpatialCoordinateSystem;
#endif

    /// <summary>
    /// Anchors the parent GameObject to a low-level static node.
    /// </summary>
    /// <param name="staticNodeGuid">Low-level anchor identifier.</param>
    public void AttachToStaticNode(System.Guid staticNodeGuid)
    {
#if WINDOWS_UWP
        // Get SpatialCoordinateSystem for static node.
        anchorSpatialCoordinateSystem = SpatialGraphInteropPreview.CreateCoordinateSystemForNode(staticNodeGuid);
#endif
    }

    /// <summary>
    /// Removes the anchoring to a low-level static node.
    /// </summary>
    /// <remarks>
    /// To temporarily pause anchoring, just disable this component instead.
    /// </remarks>
    public void DetachFromStaticNode()
    {
#if WINDOWS_UWP
        // Unset SpatialCoordinateSystem.
        anchorSpatialCoordinateSystem = null;
#endif
    }

    /// <summary>
    /// Initialization.
    /// </summary>
    void Start()
    {
#if WINDOWS_UWP
        // Get the SpatialCoordinateSystem used by Unity for the entire scene.
        unitySpatialCoordinateSystem = Marshal.GetObjectForIUnknown(WorldManager.GetNativeISpatialCoordinateSystemPtr()) as SpatialCoordinateSystem;
#endif
    }

    /// <summary>
    /// Update.
    /// </summary>
    void Update()
    {
#if WINDOWS_UWP
        // Only update if there is actually a SpatialCoordinateSystem to update to.
        if (anchorSpatialCoordinateSystem == null)
        {
            return;
        }

        // Get the current global transform of the SpatialCoordinateSystem in the Unity scene.
        System.Numerics.Matrix4x4? spatialCoordinateSystemToSceneTransform = anchorSpatialCoordinateSystem.TryGetTransformTo(unitySpatialCoordinateSystem);

        // The transform can be null if there is no tracking at the moment.
        if (spatialCoordinateSystemToSceneTransform == null)
        {
            return;
        }

        System.Numerics.Matrix4x4 anchorTransform = spatialCoordinateSystemToSceneTransform.Value;

        // Convert from right-handed to Unity's left-handed coordinate system.
        anchorTransform.M13 = -anchorTransform.M13;
        anchorTransform.M23 = -anchorTransform.M23;
        anchorTransform.M43 = -anchorTransform.M43;
        anchorTransform.M31 = -anchorTransform.M31;
        anchorTransform.M32 = -anchorTransform.M32;
        anchorTransform.M34 = -anchorTransform.M34;

        // Decompose low-level anchor transform.
        System.Numerics.Vector3 anchorTransformScale;
        System.Numerics.Quaternion anchorTransformRotation;
        System.Numerics.Vector3 anchorTransformTranslation;
        System.Numerics.Matrix4x4.Decompose(anchorTransform,
            out anchorTransformScale,
            out anchorTransformRotation,
            out anchorTransformTranslation);

        // Update the parent GameObject's transform.
        transform.SetPositionAndRotation(
            new Vector3(
                anchorTransformTranslation.X,
                anchorTransformTranslation.Y,
                anchorTransformTranslation.Z),
            new Quaternion(
                anchorTransformRotation.X,
                anchorTransformRotation.Y,
                anchorTransformRotation.Z,
                anchorTransformRotation.W));
#endif
    }
}
