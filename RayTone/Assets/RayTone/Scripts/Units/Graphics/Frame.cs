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
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.IO;

namespace RayTone
{
    public class Frame : GraphicsUnit
    {
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private RenderTexture sourceTexture;
        [SerializeField] private RenderTexture destTexture;

        private GraphicsController graphicsController;
        private float width;
        private float height;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            graphicsController = GraphicsController.Instance;
            mesh.material.mainTexture = destTexture;
            
            width = destTexture.width;
            height = destTexture.height;

            // Store texture - we Blit once before storing to "activate" the texture.
            // Otherwise, NativeTexturePtr returns 0 the first time. 
            Graphics.Blit(sourceTexture, destTexture);
            graphicsController.AddTexture(destTexture);
            graphicsController.SetTextureResolution(destTexture, width, height);  // TODO: dynamic resolution?

            mesh.transform.localScale = new(width * 0.0015f, 0.01f, height * 0.0015f);  // sweet scale multiplier...
        }

        /////
        //UPDATE
        private void Update()
        {
            Graphics.Blit(sourceTexture, destTexture);
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            graphicsController.RemoveTexture(destTexture);
        }

        /// <summary>
        /// texture ID
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            int id = 0;
            
            if (destTexture)
            {
                id = graphicsController.GetTextureID(destTexture);
            }

            StoreValue(id);
            return id;
        }
    }
}
