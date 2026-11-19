using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class McpAutoStartOnce
{
    static McpAutoStartOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool("NutriPick.McpAutoStart2", false))
                return;

            SessionState.SetBool("NutriPick.McpAutoStart2", true);
            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", false);
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            Debug.Log("[NutriPick] Configured MCP for Unity to use the stdio bridge.");
            EditorUtility.RequestScriptReload();
        };
    }
}
