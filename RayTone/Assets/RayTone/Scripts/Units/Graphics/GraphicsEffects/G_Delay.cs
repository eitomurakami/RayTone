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
using UnityEngine;

namespace RayTone
{
    public class G_Delay : GraphicsUnit_Effect
    {        
        private const int textureBufferLength = 60;
        private RenderTexture[] textureBuffer = new RenderTexture[textureBufferLength];
        private int inTextureIndex = 0;

        /////
        //START
        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < textureBufferLength; i++)
            {
                RenderTexture texture_temp = new((int)resolution.x, (int)resolution.y, 0);
                textureBuffer[i] = texture_temp;
            }
        }

        /////
        //UPDATE
        private void Update()
        {
            // No connection: Clear render texture
            if (!GetInletStatus(0))
            {
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = textureBuffer[inTextureIndex];
                GL.Clear(true, true, Color.black);
                RenderTexture.active = previous;

                Delay();
            }
        }

        /// <summary>
        /// Render frame
        /// </summary>
        protected override void RenderFrame()
        {
            // Copy in/null texture to current index
            Texture inTexture = null;
            if (GetInletStatus(0))
            {
                inTexture = graphicsController.GetTextureWithID((int)GetInletVal(0));
                if (inTexture != null)
                {
                    Graphics.Blit(inTexture, textureBuffer[inTextureIndex]);
                }
            }

            // Not a valid texture: Clear render texture
            if (inTexture == null)
            {
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = textureBuffer[inTextureIndex];
                GL.Clear(true, true, Color.black);
                RenderTexture.active = previous;
            }

            Delay();
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            graphicsController.RemoveTexture(renderTexture);
            renderTexture.Release();
            Destroy(renderTexture);

            foreach (RenderTexture texture in textureBuffer)
            {
                texture.Release();
                Destroy(texture);
            }
        }

        private void Delay()
        {
            // Get delay length
            int delayFrameLength = 0;
            if (GetInletStatus(1))
            {
                delayFrameLength = Math.Clamp((int)GetInletVal(1), 0, textureBufferLength - 1);
            }

            // Compute out index and copy texture
            int outTextureIndex = (inTextureIndex - delayFrameLength + textureBufferLength) % textureBufferLength;
            Graphics.Blit(textureBuffer[outTextureIndex], renderTexture);

            // Advance index
            inTextureIndex = (inTextureIndex + 1) % textureBufferLength;

            // Send render frame request
            NotifyQueueRenderFrame();
        }
    }
}
