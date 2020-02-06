using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;
using System;
using UnityEngine.UI;

#if WINDOWS_UWP
using System.Runtime.InteropServices;
using Windows.Perception.Spatial;
using Windows.Perception.Spatial.Preview;
#endif

public class WorldAnchorDiagnosticsLabel : MonoBehaviour
{
    public WorldAnchorDiagnosticsManager Manager;

    /// <summary>
    /// WorldAnchor object diagnostics are created for.
    /// </summary>
    [Tooltip("WorldAnchor object diagnostics are created for.")]
    public WorldAnchor WorldAnchor;

    /// <summary>
    /// TextMesh object anchor diagnostics are appended to.
    /// </summary>
    [Tooltip("TextMesh object anchor diagnostics are appended to.")]
    public TextMesh TextMesh;

    /// <summary>
    /// Text format for the original label text and anchor diagnostics.
    /// <list type="table">
    ///     <item>
    ///         <term>{0}</term>
    ///         <description>Initial text of the TextMesh prior to the first update.</description>
    ///     </item>
    ///     <item>
    ///         <term>{1}</term>
    ///         <description>Identifier number (a small integer) of the underlying low-level anchor.</description>
    ///     </item>
    ///     <item>
    ///         <term>{2}</term>
    ///         <description>Distance (in meters) between the WorldAnchor and its underlying low-level anchor.</description>
    ///     </item>
    /// </list>
    /// </summary>
    [Tooltip("Text format for the original label text and anchor diagnostics: {0} = initial label, {1} = low-level anchor number, {2} = distance from low-level anchor")]
    public string TextFormat = "{0} (A{1} {2:f1}m)";

    /// <summary>
    /// Wait time between diagnostics updates.
    /// </summary>
    [Tooltip("Wait time between diagnostics updates. (Actual changes of the displayed diagnostics are very rare.)")]
    public float UpdateInterval = 10.0f;

    /// <summary>
    /// Minimum change in distance to low-level anchor origin to prompt an update of the label text.
    /// </summary>
    [Tooltip("Minimum change (in meters) of the distance to the low-level anchor that will prompt an update of the label text.")]
    public float UpdateDistanceThreshold = 0.05f;

#if WINDOWS_UWP
    /// <summary>
    /// Text of the TextMesh object at start.
    /// </summary>
    private string initialText;

    /// <summary>
    /// Most recently rendered value of the friendly low-level anchor identifier.
    /// </summary>
    private int lastSpatialNodeFriendlyId = -1;
    
    /// <summary>
    /// Most recently rendered value of the WorldAnchor's distance from the underlying low-level anchor.
    /// </summary>
    private float lastSpatialAnchorDistance = -1.0f;
#endif

    // Start is called before the first frame update
    void Start()
    {
#if WINDOWS_UWP
        initialText = TextMesh.text;
        StartCoroutine(UpdateDiagnosticsContinuously());
#endif
    }

#if WINDOWS_UWP
    private IEnumerator UpdateDiagnosticsContinuously()
    {
        while (true)
        {
            SpatialAnchor spatialAnchor = Marshal.GetObjectForIUnknown(WorldAnchor.GetNativeSpatialAnchorPtr()) as SpatialAnchor;
            SpatialGraphInteropFrameOfReferencePreview spatialFrameOfReference = SpatialGraphInteropPreview.TryCreateFrameOfReference(spatialAnchor.CoordinateSystem);

            int spatialNodeFriendlyId = Manager.GetSpatialNodeFriendlyId(spatialFrameOfReference.NodeId);
            float spatialAnchorDistance = spatialFrameOfReference.CoordinateSystemToNodeTransform.Translation.Length();

            if (spatialNodeFriendlyId != lastSpatialNodeFriendlyId || Math.Abs(spatialAnchorDistance - lastSpatialAnchorDistance) <= UpdateDistanceThreshold)
            {
                TextMesh.text = string.Format(TextFormat, initialText, spatialNodeFriendlyId, spatialAnchorDistance);

                lastSpatialNodeFriendlyId = spatialNodeFriendlyId;
                lastSpatialAnchorDistance = spatialAnchorDistance;
            }

            // Debug.unityLogger.Log(string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] WorldAnchorDiagnosticsLabel updated TextMesh \"{1}\".", DateTime.Now, TextMesh.text));

            yield return new WaitForSeconds(UpdateInterval * UnityEngine.Random.Range(0.95f, 1.05f));
        }
    }
#endif
}
