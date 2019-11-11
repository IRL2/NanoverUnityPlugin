using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Frame.Import.CIF;
using Narupa.Frame.Import.CIF.Components;
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

        private static string structureId = "";
        private static string currentTitle = "";

        private static string currentFile = "";
        private static bool downloading = false;


        /// <summary>
        /// Show a progress message, potentially from another thread.
        /// </summary>
        /// <param name="threadSafe"></param>
        private static void ShowProgressMessage(string message, bool threadSafe)
        {
            if (threadSafe)
            {
                EditorUtility.DisplayProgressBar("Import File", message, 0.5f);
                queuedProgressMessage = null;
            }
            else
            {
                queuedProgressMessage = message;
            }
        }

        private static void HideProgressMessage()
        {
            queuedProgressMessage = null;
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Callback for drawing a GUI during a prefab view
        /// </summary>
        private static void SceneViewOnDuringSceneGui(SceneView obj)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
                return;
            
            if(!hasInitialised)
                OnPrefabStageOpened(PrefabStageUtility.GetCurrentPrefabStage());

            if (!string.IsNullOrEmpty(queuedProgressMessage))
            {
                ShowProgressMessage(queuedProgressMessage, true);
            }

            if (!isValidPrefabForVisualisation)
                return;

            DrawGUI();
        }

        private static void DrawGUI()
        {
            Handles.BeginGUI();

            var titleWidth = EditorStyles.miniBoldLabel.CalcSize(new GUIContent(currentTitle)).x;

            GUILayout.BeginArea(
                new Rect(4, 4, 2 + 48 + 2 + 36 + 4 + 76 + 4 + 4 + 76 + titleWidth + 4 + 46, 32),
                GUIContent.none,
                EditorStyles.toolbar);
            GUI.Label(new Rect(2, 0, 48, 28),
                      "Preview",
                      EditorStyles.miniBoldLabel);
            structureId = GUI.TextField(new Rect(2 + 48 + 2, 2, 36, 28),
                                        structureId,
                                        EditorStyles.toolbarTextField);
            structureId = structureId.Trim().ToUpper();
            if (GUI.Button(new Rect(2 + 48 + 2 + 36 + 4, 0, 76 + 40, 32),
                           "Load Structure",
                           EditorStyles.toolbarButton))
            {
                LoadCifFileFromRcsb(structureId);
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

        /// <summary>
        /// Is the current prefab a valid visualiser.
        /// </summary>
        private static bool isValidPrefabForVisualisation = false;

        /// <summary>
        /// <see cref="OnPrefabStageOpened"/> is not called on recompile
        /// </summary>
        private static bool hasInitialised = false;
        
        private static void OnPrefabStageOpened(PrefabStage prefabStage)
        {
            hasInitialised = true;
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
                {
                    LoadFile(currentFile);
                }
            }
            catch (Exception e)
            {
                // Need to catch all exceptions here, otherwise Editor can bug out.
                CancelPrefabView();
                ShowExceptionAndLog(e);
            }
        }

        private static void CancelPrefabView()
        {
            isValidPrefabForVisualisation = false;
            HideProgressMessage();
        }

        private static void LoadCifFileFromRcsb(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            currentFile = $"{Application.temporaryCachePath}/{id}.cif";

            if (File.Exists(currentFile))
            {
                try
                {
                    LoadFile(currentFile);
                }
                catch (Exception e)
                {
                    CancelPrefabView();
                    ShowExceptionAndThrow(e);
                }
            }
            else
            {
                var client = new WebClient();
                client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
                client.CancelAsync();
                client.DownloadFileAsync(new Uri($"https://files.rcsb.org/download/{id}.cif"),
                                         currentFile);
                ShowProgressMessage("Downloading file", true);
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
                    CancelPrefabView();
                    ShowExceptionAndThrow(exception);
                }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                CancelPrefabView();
                ShowExceptionAndThrow(e.Error);
            }
        }

        private static void ShowExceptionAndThrow(Exception e)
        {
            EditorUtility.DisplayDialog("Exception", e.Message, "OK");
            throw e;
        }

        private static void ShowExceptionAndLog(Exception e)
        {
            EditorUtility.DisplayDialog("Exception", e.Message, "OK");
            Debug.LogException(e);
        }

        private class Progress : IProgress<string>
        {
            public void Report(string value)
            {
                ShowProgressMessage(value, false);
            }
        }

        private static string queuedProgressMessage = "";

        private static async Task<Frame.Frame> ImportCifFile(
            string filename,
            ChemicalComponentDictionary dictionary)
        {
            using (var file = File.OpenRead(filename))
            using (var reader = new StreamReader(file))
            {
                if (Path.GetExtension(filename).Contains("cif"))
                    return CifImport.Import(reader, dictionary, new Progress());
            }

            return null;
        }

        private static async void LoadFile(string filename)
        {
            EditorUtility.DisplayProgressBar("Load File", "...", 0f);

            var dictionary = ChemicalComponentDictionary.Instance;

            var frame = await Task.Run(() => ImportCifFile(filename, dictionary));

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

            structureId = Path.GetFileNameWithoutExtension(currentFile);
            HideProgressMessage();
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