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
using UnityEngine;

namespace RayTone
{
    public class GraphicsUnit_Effect : GraphicsUnit
    {
        [Header("Graphics Effects")]
        [SerializeField] private List<InletSocket> textureInlets;
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private Shader shader;

        protected Vector2 resolution = new(1920, 1080);
        protected FilterMode filterMode = FilterMode.Bilinear;
        protected GraphicsController graphicsController;
        protected RenderTexture renderTexture;
        protected Material material;
        private int textureIndex = -1;
        private List<(InletSocket, bool)> textureInletsStatus = new();
        private bool renderedOnce = false;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            graphicsController = GraphicsController.Instance;

            // Initialize texture inlets status
            foreach (InletSocket inlet in textureInlets)
            {
                textureInletsStatus.Add((inlet, false));
            }

            // Iniitailze material
            material = new(shader);

            // Initialize RenderTexture
            renderTexture = new((int)resolution.x, (int)resolution.y, 0);
            renderTexture.filterMode = filterMode;
            renderTexture.wrapMode = TextureWrapMode.Repeat;
            renderTexture.Create();
            graphicsController.AddTexture(renderTexture);
            graphicsController.SetTextureResolution(renderTexture, resolution.x, resolution.y);

            // Set plane texture
            mesh.material.mainTexture = renderTexture;
            SetResolution(resolution, false);
        
        }

        /////
        //UPDATE
        private void Update()
        {
            if (textureIndex != 0 && !GetInletStatus(0))
            {
                // Reset to null texture
                textureIndex = 0;
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = previous;
            }
        }

        /////
        //LATE-UPDATE
        private void LateUpdate()
        {
            // Reset texture inlets status
            for (int i = 0; i < textureInlets.Count; i++)
            {
                textureInletsStatus[i] = (textureInletsStatus[i].Item1, false);
            }
            renderedOnce = false;
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            graphicsController.RemoveTexture(renderTexture);
            Destroy(material);
            renderTexture.Release();
            Destroy(renderTexture);
        }

        /// <summary>
        /// Render frame
        /// </summary>
        protected virtual void RenderFrame()
        {
            if (!GetInletStatus(0)) { return; }

            int inletVal = (int)GetInletVal(0);
            if (textureIndex != inletVal)
            {
                // Retrieve source texture
                textureIndex = inletVal;
                Texture texture = graphicsController.GetTextureWithID(textureIndex);
                (float, float) textureResolution = graphicsController.GetTextureResolution(texture);

                // Update material
                material.SetTexture("_MainTex", texture);
                graphicsController.SetTextureResolution(renderTexture, textureResolution.Item1, textureResolution.Item2);
            }

            // Render frame with custom shader
            Graphics.Blit(null, renderTexture, material, 0);

            // Send render frame request
            NotifyQueueRenderFrame();
        }

        /// <summary>
        /// texture ID
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            int id = 0;
            
            if (renderTexture)
            {
                id = graphicsController.GetTextureID(renderTexture);
            }

            StoreValue(id);
            return id;
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            base.ApplyUnitProperties(up);
            if (up.metaInt.ContainsKey("resolution_x"))
            {
                resolution.x = up.metaInt["resolution_x"];
            }
            if (up.metaInt.ContainsKey("resolution_y"))
            {
                resolution.y = up.metaInt["resolution_y"];
            }
            if (up.metaInt.ContainsKey("filter"))
            {
                filterMode = (FilterMode)up.metaInt["filter"];
            }
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = base.GetUnitProperties();
            up.metaInt = new();
            up.metaInt.Add("resolution_x", (int)resolution.x);
            up.metaInt.Add("resolution_y", (int)resolution.y);
            up.metaInt.Add("filter", (int)filterMode);

            return up;
        }

        /// <summary>
        /// Get render texture resolution
        /// </summary>
        /// <returns></returns>
        public Vector2 GetResolution()
        {
            return resolution;
        }

        /// <summary>
        /// Resize render texture
        /// </summary>
        /// <param name="newResolution"></param>
        public void SetResolution(Vector2 newResolution, bool reinitTexture=true)
        {
            resolution = newResolution;

            // Reinitialize render texture
            if (reinitTexture)
            {
                renderTexture.Release();
                renderTexture.width = (int)resolution.x;
                renderTexture.height = (int)resolution.y;
                renderTexture.Create();
            }

            graphicsController.SetTextureResolution(renderTexture, resolution.x, resolution.y);
            if ((resolution.x / resolution.y) > 1.77)  // 16:9 (2.88:1.62) by default
            {
                mesh.transform.localScale = new(2.88f, 0.01f, 2.88f * resolution.y / resolution.x);
            }
            else
            {
                mesh.transform.localScale = new(1.62f * resolution.x / resolution.y, 0.01f, 1.62f);
            }
        }

        /// <summary>
        /// Get render texture filter mode (0: point, 1: bilinear)
        /// </summary>
        /// <returns></returns>
        public int GetFilterMode()
        {
            return (int)filterMode;
        }

        /// <summary>
        /// Set render texture filter mode (0: point, 1: bilinear)
        /// </summary>
        /// <param name="arg"></param>
        public void SetFilterMode(int arg)
        {
            filterMode = (FilterMode)arg;
            renderTexture.filterMode = filterMode;
        }

        /// <summary>
        /// Keep track of texture inlets status and render frame when all of them are queued.
        /// </summary>
        /// <param name="inlet"></param>
        public override void QueueRenderFrame(InletSocket inlet)
        {
            // No texture inlets queue required
            if (renderedOnce || textureInletsStatus.Count == 0) { return; }

            bool textureInletFound = false;
            bool allTextureInletsQueued = true;
            for(int i = 0; i < textureInlets.Count; i++)
            {
                // Queue texture inlet
                if (textureInletsStatus[i].Item1 == inlet)
                {
                    textureInletsStatus[i] = (textureInletsStatus[i].Item1, true);
                    textureInletFound = true;
                }

                // Are all texture inlets queued?
                if (!textureInletsStatus[i].Item2 && GetInletStatus(i))
                {
                    allTextureInletsQueued = false;
                }
            }

            // Render
            if (textureInletFound && allTextureInletsQueued)
            {
                renderedOnce = true;
                RenderFrame();
            }
        }
    }
}
