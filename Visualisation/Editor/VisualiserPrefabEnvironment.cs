using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Frame.Import.CIF;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    [InitializeOnLoad]
    internal static class VisualiserPrefabEnvironment
    {
        static VisualiserPrefabEnvironment()
        {
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            SceneView.duringSceneGui += SceneViewOnDuringSceneGui;

            EditorUtility.ClearProgressBar();
        }

        private static string structure = "";
        private static string currentTitle = "";

        private static void SceneViewOnDuringSceneGui(SceneView obj)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
                return;

            if (!isValidPrefabForVisualisation)
                return;

            Handles.BeginGUI();

            var titleWidth = EditorStyles.miniBoldLabel.CalcSize(new GUIContent(currentTitle)).x;

            GUILayout.BeginArea(
                new Rect(4, 4, 2 + 48 + 2 + 36 + 4 + 76 + 4 + 4 + 76 + titleWidth + 4 + 46, 32),
                GUIContent.none,
                EditorStyles.toolbar);
            GUI.Label(new Rect(2, 0, 48, 28),
                      "Preview",
                      EditorStyles.miniBoldLabel);
            structure = GUI.TextField(new Rect(2 + 48 + 2, 2, 36, 28),
                                      structure,
                                      EditorStyles.toolbarTextField);
            structure = structure.Trim().ToUpper();
            if (GUI.Button(new Rect(2 + 48 + 2 + 36 + 4, 0, 76 + 40, 32),
                           "Load Structure",
                           EditorStyles.toolbarButton))
            {
                LoadProtein(structure);
            }

            var x = 2 + 48 + 2 + 36 + 4 + 72 + 4 + 40;

            GUI.enabled = currentMolecule != null;

            if (GUI.Button(new Rect(x, 0, 76, 32),
                           "Show File",
                           EditorStyles.toolbarButton))
            {
                EditorUtility.RevealInFinder(currentFile);
            }

            GUI.enabled = true;

            GUI.Label(new Rect(2 + 48 + 2 + 36 + 4 + 72 + 4 + 76 + 4, 0, titleWidth, 28),
                      currentTitle,
                      EditorStyles.miniBoldLabel);

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static bool isValidPrefabForVisualisation = false;

        private static void OnPrefabStageOpened(PrefabStage prefabStage)
        {
            try
            {
                if (prefabStage.prefabContentsRoot.GetComponentInChildren<IFrameConsumer>() == null)
                {
                    isValidPrefabForVisualisation = false;
                    return;
                }

                isValidPrefabForVisualisation = true;
                currentFile = EditorPrefs.GetString("visualiser.prefab.file");
                if (currentFile != null && File.Exists(currentFile))
                    LoadFile(currentFile);
            }
            catch (Exception e)
            {
                // Need to catch all exceptions here, otherwise Editor can bug out.
                Debug.LogException(e);
            }
        }

        private static string currentFile = "";

        private static bool downloading = false;

        private static void LoadProtein(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            currentFile = $"{Application.temporaryCachePath}/{id}.cif";

            if (File.Exists(currentFile))
            {
                LoadFile(currentFile);
            }
            else
            {
                var client = new WebClient();
                client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
                client.CancelAsync();
                client.DownloadFileAsync(new Uri($"https://files.rcsb.org/download/{id}.cif"),
                                         currentFile);
                EditorUtility.DisplayProgressBar("Loading CIf", "Downloading file", 0.33f);
                downloading = true;
            }
        }

        private static void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloading = false;
            if (e.Error == null)
            {
                EditorUtility.DisplayProgressBar("Loading CIF", "Importing file", 0.66f);
                try
                {
                    LoadFile(currentFile);
                }
                catch (Exception exception)
                {
                    ShowException(exception);
                }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.ClearProgressBar();
                ShowException(e.Error);
            }
        }

        private static void ShowException(Exception e)
        {
            EditorUtility.DisplayDialog("Exception", e.Message, "OK");
            throw e;
        }

        private static void LoadFile(string filename)
        {
            Frame.Frame frame;

            using (var file = File.OpenRead(filename))
            using (var reader = new StreamReader(file))
            {
                if (Path.GetExtension(filename).Contains("cif"))
                    frame = CifImport.Import(reader);
                else
                    return;
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var root = prefabStage.prefabContentsRoot;
            var renderer = root.GetComponent<IFrameConsumer>();
            if (renderer == null)
                return;
            currentTitle = frame.Data.GetValueOrDefault<string>("title") ?? "";

            frame.RecenterAroundOrigin();
            currentMolecule = new FrameSource(frame);
            renderer.FrameSource = currentMolecule;
            root.gameObject.SendMessage("Update");

            EditorPrefs.SetString("visualiser.prefab.file", currentFile);

            structure = Path.GetFileNameWithoutExtension(currentFile);
        }

        private static FrameSource currentMolecule = null;

        private class FrameSource : ITrajectorySnapshot
        {
            public FrameSource(Frame.Frame frame)
            {
                CurrentFrame = frame;
            }

            public Frame.Frame CurrentFrame { get; }
            public event FrameChanged FrameChanged;
        }
    }
}