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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayTone
{
    public class GraphicsController : Singleton<GraphicsController>
    {        
        [SerializeField] private ShaderRenderer shaderRenderer;
        [SerializeField] private MeshRenderer renderPlane;
        [SerializeField] private Texture nullTexture;

        private Dictionary<Texture, (IntPtr, float, float)> textures = new();
        // Rendering=VFX unit | Windowing=Window unit
        // Window status always overwrites Rendering status.
        private bool isRendering = false;
        private bool isWindowing = false;

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();

            SetRenderingStatus(false);
        }

        /////
        //START
        private void Start()
        {
            AddTexture(nullTexture);
            SetRenderingQualityDivider(2);  // QHD default
        }

        /// <summary>
        /// Set rendering status
        /// </summary>
        /// <param name="status"></param>
        public void SetRenderingStatus(bool status)
        {
            isRendering = status;
            renderPlane.enabled = isRendering || isWindowing;
            shaderRenderer.SetRenderingStatus(isRendering && !isWindowing);
        }
        
        /// <summary>
        /// Set windowing status
        /// </summary>
        /// <param name="status"></param>
        public void SetWindowingStatus(bool status)
        {
            isWindowing = status;
            renderPlane.enabled = isRendering || isWindowing;
            shaderRenderer.SetRenderingStatus(isRendering && !isWindowing);
        }

        /// <summary>
        /// Update shader rendering quality level.
        /// 1=UHD, 2=QHD, 3=FHD, 4=HD.
        /// </summary>
        /// <param name="divider"></param>
        public void SetRenderingQualityDivider(int divider)
        {
            Vector2 resolution = default;
            switch (divider)
            {
                case 1:
                    resolution = new Vector2(3840f, 2160f);  // UHD
                    break;
                case 2:
                    resolution = new Vector2(2560f, 1440f);  // QHD
                    break;
                case 3:
                    resolution = new Vector2(1920f, 1080f);  // FHD
                    break;
                case 4:
                    resolution = new Vector2(1280f, 720f);  // HD
                    break;
            }
            Console.Log("Setting rendering resolution to: " + resolution.x + " x " + resolution.y);
            shaderRenderer.SetRenderingResolution((int)resolution.x, (int)resolution.y);
        }

        /// <summary>
        /// Call SetFragmentShaderFilePath in ShaderRenderer class
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFragmentShaderFilePath(string filePath)
        {
            shaderRenderer.SetFragmentShaderFilePath(filePath);
        }

        /// <summary>
        /// Set inlets in ShaderRenderer
        /// </summary>
        /// <param name="index"></param>
        /// <param name="val"></param>
        public void SetInlet(int inletIndex, float val)
        {
            shaderRenderer.SetInlet(inletIndex, val);
        }

        /// <summary>
        /// Set textures in ShaderRenderer
        /// </summary>
        /// <param name="textureIndex"></param>
        /// <param name="texturePtr"></param>
        public void SetTexture(int textureIndex, IntPtr texturePtr, float width, float height)
        {
            shaderRenderer.SetTexture(textureIndex, texturePtr, width, height);
        }

        /// <summary>
        /// Add a texture entry
        /// </summary>
        /// <param name="texture"></param>
        public void AddTexture(Texture texture)
        {
            textures.Add(texture, (texture.GetNativeTexturePtr(), 0f, 0f));
        }

        /// <summary>
        /// Remove a texture entry
        /// </summary>
        /// <param name="texture"></param>
        public void RemoveTexture(Texture texture)
        {
            textures.Remove(texture);
        }

        /// <summary>
        /// Get the index of a texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public int GetTextureID(Texture texture)
        {
            if (textures.ContainsKey(texture))
            {
                return textures.Keys.ToList().IndexOf(texture);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get texture reference
        /// </summary>
        /// <param name="textureID"></param>
        /// <returns></returns>
        public Texture GetTextureWithID(int textureID)
        {
            if (textureID >= 0 && textureID < textures.Count)
            {
                return textures.Keys.ToList()[textureID];
            }
            else
            {
                return textures.Keys.ToList()[0];
            }
        }

        /// <summary>
        /// Get stored native texture pointer
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public IntPtr GetTexturePtr(Texture texture)
        {
            if (textures.ContainsKey(texture))
            {
                return textures[texture].Item1;
            }

            return (IntPtr)0;
        }

        /// <summary>
        /// Set texture resolution
        /// </summary>
        /// <param name="textureID"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetTextureResolution(Texture texture, float width, float height)
        {
            textures[texture] = (textures[texture].Item1, width, height);
        }

        /// <summary>
        /// Get texture resolution
        /// </summary>
        /// <param name="textureID"></param>
        /// <returns></returns>
        public (float, float) GetTextureResolution(Texture texture)
        {
            if (textures.ContainsKey(texture))
            {
                return (textures[texture].Item2, textures[texture].Item3);
            }

            return (0f, 0f);
        }
    }
}
