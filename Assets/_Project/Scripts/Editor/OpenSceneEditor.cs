using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSceneEditor : EditorWindow {
	[MenuItem("Project-Tools/All Scenes &%s")]
	public static void CreateWindows()
	{
		EditorWindow editorWindow = GetWindow (typeof(OpenSceneEditor), false, "All Scenes");
		editorWindow.BeginWindows ();
	}

	string [] scenesBuild;
    string [] allScenePaths;

    bool onlySceneBuild = true;

	private void OnEnable ()
	{
        LoadData ();
    }

    private void LoadData ()
    {
        var scenes = EditorBuildSettings.scenes;
        scenesBuild = new string [scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenesBuild [i] = scenes [i].path;
        }

        var guiIDs = AssetDatabase.FindAssets ("t:Scene");
        allScenePaths = new string [guiIDs.Length];

        for (int i = 0; i < guiIDs.Length; i++)
        {
            var guiID = guiIDs [i];
            allScenePaths [i] = AssetDatabase.GUIDToAssetPath (guiID);
        }
    }

    private void OnGUI()
	{
        LoadData ();

        GUILayout.BeginVertical ();

        if (allScenePaths.Length == 0)
        {
            GUILayout.Label ("No scenes in Project");
            return;
        }

        onlySceneBuild = EditorGUILayout.Toggle ("Only Build Scene", onlySceneBuild);
        GUILayout.Space (20);

        for (int i = 0; i < allScenePaths.Length; i++)
        {
            var path = allScenePaths [i];

            if (onlySceneBuild && !scenesBuild.Contains (path))
                continue;

            DisplayScene (path);
        }

        GUILayout.EndVertical ();

        GUILayout.FlexibleSpace ();
        if (GUILayout.Button ("Reload"))
            OnEnable ();
    }

	private void DisplayScene (string path)
	{
		string[] part = path.Split ('/');

        GUILayout.BeginHorizontal (GUI.skin.box);
        var sceneName = part [part.Length - 1].Replace (".unity", "");
        GUILayout.Label (sceneName, GUILayout.Width (120));

        GUILayout.FlexibleSpace ();

        if (GUILayout.Button ("Open", GUILayout.ExpandWidth (false)))
        {
            if (EditorApplication.isPlaying)
            {
                SceneManager.LoadScene (sceneName);
            }
            else
            {
                EditorSceneManager.OpenScene (path);
            }
        }

        if (GUILayout.Button ("Find", GUILayout.ExpandWidth (false)))
        {
            EditorGUIUtility.PingObject (AssetDatabase.LoadAssetAtPath<SceneAsset> (path));
        }

        GUILayout.EndHorizontal ();
    }
}
