/*----------------------------------------------------------------------------
*  RayTone: A Node-based Audiovisual Sequencing Environment
*      https://www.raytone.app/
*
*  Copyright 2024 Eito Murakami and John Burnett
*
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
-----------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTone
{
    public class ShaderRenderer : MonoBehaviour
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] delegate void FuncPtr(string str);

        [DllImport("RayToneShaderRenderer")] private static extern void RegisterErrorLogCallback([MarshalAs(UnmanagedType.FunctionPtr)] FuncPtr callback);
        [DllImport("RayToneShaderRenderer")] private static extern System.IntPtr Execute();
        [DllImport("RayToneShaderRenderer")] private static extern void SetFragmentShaderText(string fragShaderText);
        [DllImport("RayToneShaderRenderer")] private static extern void SetTime(float time);
        [DllImport("RayToneShaderRenderer")] private static extern void SetInletVal(int inletIndex, float val);
        [DllImport("RayToneShaderRenderer")] private static extern void SetTexturePointer(int textureIndex, System.IntPtr texturePtr, float width, float height);
        [DllImport("RayToneShaderRenderer")] private static extern void SetResolution(int width, int height);

        [SerializeField] RenderTexture targetRenderTexture;
        [SerializeField] Shader fullscreenShader;

        private RayToneController raytoneController;
        private CameraController cameraController;
        private Material fullscreenMaterial;
        private bool isRendering;
        private float[] inlets = new float[8];
        private string fragmentShaderFilePath = "";
        private string fragmentShaderText = "";
        private float elapsedTime = 0;
        private float shaderRefreshRate = 2f;
        private System.DateTime lastEdit;

        // error log from plugin
        private static List<string> logStream = new();

        /////
        //START
        void Start()
        {
            // Initialize material to use with Blit when in full-screen mode
            fullscreenMaterial = new(fullscreenShader);
            fullscreenMaterial.SetTexture("_MainTex", targetRenderTexture);

            raytoneController = RayToneController.Instance;
            cameraController = CameraController.Instance;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;

#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX || UNITY_IOS
            return;
#else

            FuncPtr errorLogCallback = ErrorLog;

            RegisterErrorLogCallback(errorLogCallback);
            GL.IssuePluginEvent(Execute(), 0);  // Initialize
            StartCoroutine("CallNativePlugin");
#endif
        }

        /////
        //UPDATE
        void Update()
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX || UNITY_IOS
            return;
#else
            // Process error log from the plugin
            while (logStream.Count > 0)
            {
                Console.Log(logStream[0]);
                if(logStream[0].Contains("error"))
                {
                    Console.SetConsoleVisibility(true);
                }
                logStream.RemoveAt(0);
            }

            // If rendering, check fragment shader text at the refresh rate and compile if there are changes.
            if (!isRendering) return;

            elapsedTime += Time.deltaTime;
            if (elapsedTime < shaderRefreshRate) return;

            elapsedTime = 0;
            System.DateTime editCheck = File.GetLastWriteTime(fragmentShaderFilePath);
            if (lastEdit == editCheck) return;
 
            lastEdit = editCheck;
            fragmentShaderText = File.ReadAllText(fragmentShaderFilePath);
            if (!string.IsNullOrEmpty(fragmentShaderText))
            {
                SetFragmentShaderText(fragmentShaderText);
                GL.IssuePluginEvent(Execute(), 1);  // Compile shaders
            }
#endif
        }

        /// <summary>
        /// Update plugin at the end of every frame
        /// </summary>
        /// <returns></returns>
        IEnumerator CallNativePlugin()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (isRendering && !string.IsNullOrEmpty(fragmentShaderText))
                {
                    Graphics.SetRenderTarget(targetRenderTexture);
                    SetTime(Time.time);
                    for(int i = 0; i < inlets.Length; i++)
                    {
                        SetInletVal(i, inlets[i]);
                    }
                    GL.IssuePluginEvent(Execute(), 2);  // Render frame
                }
            }
        }

        /// <summary>
        /// Blit targetRenderTexture to main camera when in "performance mode"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cameras"></param>
        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (!cameraController.GetVisibility() && raytoneController.GetStoredUserConfig().fullScreen)
            {
                float scale = (Screen.width / (float)Screen.height) / (16f / 9f); // adaptive scaling of blit texture using 16:9 as default
                fullscreenMaterial.SetFloat("_Scale", scale);
                Graphics.Blit(null, Camera.main.targetTexture, fullscreenMaterial, 0);
            }
        }

        /// <summary>
        /// Handle error log from the plugin
        /// </summary>
        /// <param name="errorLog"></param>
        private static void ErrorLog(string errorLog)
        {
            logStream.Add(errorLog);
        }

        /// <summary>
        /// Set rendering status
        /// </summary>
        /// <param name="b"></param>
        public void SetRenderingStatus(bool b)
        {
            isRendering = b;
        }

        /// <summary>
        /// Set rendering resolution
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetRenderingResolution(int width, int height)
        {
            targetRenderTexture.Release();
            targetRenderTexture.width = width;
            targetRenderTexture.height = height;
            targetRenderTexture.Create();
            SetResolution(width, height);
        }

        /// <summary>
        /// Set fragment shader file absolute path
        /// </summary>
        public void SetFragmentShaderFilePath(string filePath)
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX || UNITY_IOS
            return;
#else
            fragmentShaderFilePath = filePath;
            fragmentShaderText = File.ReadAllText(fragmentShaderFilePath);

            if (!string.IsNullOrEmpty(fragmentShaderText))
            {
                SetFragmentShaderText(fragmentShaderText);
                GL.IssuePluginEvent(Execute(), 1);  // Compile shaders
            }
#endif
        }

        /// <summary>
        /// Set inlet value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="val"></param>
        public void SetInlet(int index, float val)
        {
            inlets[index] = val;
        }

        /// <summary>
        /// Set texture pointer
        /// </summary>
        /// <param name="textureIndex"></param>
        /// <param name="texturePtr"></param>
        public void SetTexture(int textureIndex, System.IntPtr texturePtr, float width, float height)
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX || UNITY_IOS
            return;
#else
            SetTexturePointer(textureIndex, texturePtr, width, height);
#endif
        }
    }
}
