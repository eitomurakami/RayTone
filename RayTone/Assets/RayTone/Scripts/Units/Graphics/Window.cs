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

namespace RayTone
{
    public class Window : GraphicsUnit
    {
        [SerializeField] RenderTexture targetRenderTexture;
        private GraphicsController graphicsController;
        private Texture texture = null;
        private int textureIndex = -1;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            graphicsController = GraphicsController.Instance;
            graphicsController.SetWindowingStatus(true);
        }

        /////
        //ON DESTROY
        protected virtual void OnDestroy()
        {
            graphicsController.SetWindowingStatus(false);
        }

        /////
        //UPDATE
        protected virtual void Update()
        {
            if (GetInletStatus(0))
            {
                int inletVal = (int)GetInletVal(0);
                if (textureIndex != inletVal)
                {
                    // Retrieve source texture
                    textureIndex = inletVal;
                    texture = graphicsController.GetTextureWithID(textureIndex);
                }

                // Copy texture to TargetRenderTexture
                Graphics.Blit(texture, targetRenderTexture);
            }
            else
            {
                // Reset to null texture
                if (textureIndex != 0)
                {
                    textureIndex = 0;
                    Graphics.Blit(graphicsController.GetTextureWithID(textureIndex), targetRenderTexture);
                }
            }
        }
    }
}
