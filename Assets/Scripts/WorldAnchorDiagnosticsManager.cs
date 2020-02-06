using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class WorldAnchorDiagnosticsManager
{
    /// <summary>
    /// Maps low-level anchor identifiers to assigned friendly numeric anchor identifiers.
    /// </summary>
    private Dictionary<System.Guid, int> staticNodeFriendlyIds = new Dictionary<System.Guid, int>();

    /// <summary>
    /// Attaches WorldAnchor diagnostics to an existing TextMesh.
    /// </summary>
    /// <remarks>
    /// Uses the first existing WorldAnchor and the first TextMesh component found in the given GameObject or any of its children. If none are found, nothing is done.
    /// </remarks>
    /// <param name="go">GameObject the diagnostics are attached to.</param>
    /// <returns>True if WorldAnchor and TextMesh components were found and diagnostics were attached, or false if not.</returns>
    public bool AttachLabel(GameObject go)
    {
#if WINDOWS_UWP
        WorldAnchor worldAnchor = go.GetComponentInChildren<WorldAnchor>();

        if (worldAnchor == null)
        {
            // Debug.unityLogger.Log("WorldAnchorDiagnosticsLabel found no WorldAnchor.");
            return false;
        }

        TextMesh textMesh = go.GetComponentInChildren<TextMesh>();

        if (textMesh == null)
        {
            // Debug.unityLogger.Log("WorldAnchorDiagnosticsLabel found no TextMesh.");
            return false;
        }

        WorldAnchorDiagnosticsLabel label = go.AddComponent<WorldAnchorDiagnosticsLabel>();

        label.Manager = this;
        label.WorldAnchor = worldAnchor;
        label.TextMesh = textMesh;

        // Debug.unityLogger.Log(string.Format("WorldAnchorDiagnosticsLabel attached to TextMesh \"{0}\".", textMesh.text));
        return true;
#else
        // Debug.unityLogger.Log("WorldAnchorDiagnosticsLabel is disabled.");
        return false;
#endif
    }

    /// <summary>
    /// Assigns a friendly numeric identifier to a low-level anchor.
    /// </summary>
    /// <param name="staticNodeGuid">Low-level anchor identifier.</param>
    /// <returns>Friendly numeric identifier (a small integer) unique to the low-level anchor.</returns>
    public int GetStaticNodeFriendlyId(System.Guid staticNodeGuid)
    {
        int spatialNodeFriendlyId;

        if (!staticNodeFriendlyIds.TryGetValue(staticNodeGuid, out spatialNodeFriendlyId))
        {
            spatialNodeFriendlyId = staticNodeFriendlyIds.Count + 1;
            staticNodeFriendlyIds.Add(staticNodeGuid, spatialNodeFriendlyId);
        
            Debug.unityLogger.Log(string.Format("Assigned friendly ID {0} to static node {1}", spatialNodeFriendlyId, staticNodeGuid));
        }

        return spatialNodeFriendlyId;
    }
}
