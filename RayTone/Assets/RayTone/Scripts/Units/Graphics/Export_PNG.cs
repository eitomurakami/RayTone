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
using System.IO;
using UnityEngine;

namespace RayTone
{
    public class Export_PNG : GraphicsUnit
    {
        private GraphicsController graphicsController;
        private bool stepUpdate = false;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            StartStepListener();

            graphicsController = GraphicsController.Instance;
        }

        /////
        //UPDATE
        private void Update()
        {
            if (stepUpdate)
            {
                GetInletVal(1); // Force recursive inlet update

                // Export PNG on trigger
                if (GetInletStatus(1) && inlets[1].connectedUnit.UpdateTrigger() == 1)
                {
                    Export((int)GetInletVal(0));
                }
                stepUpdate = false;
            }
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
        }

        // quantized tick
        public override void Step()
        {
            stepUpdate = true;
        }

        /// <summary>
        /// Export PNG from a RayTone Graphics Texture to BASE_DIR + /Export
        /// </summary>
        /// <param name="texIndex"></param>
        private void Export(int texIndex)
        {
            // Get and verify texture
            Texture texture = graphicsController.GetTextureWithID(texIndex);
            if (texture == null) return;

            // Decalre temporary Texture2D
            Texture2D copyTexture;

            // Copy RAYTONE_TEXTURE into temporary Texture2D
            // source is RenderBuffer
            if (texture.GetType() == typeof(RenderTexture)) 
            {
                copyTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false);

                RenderTexture activeRT = RenderTexture.active;
                RenderTexture.active = (RenderTexture)texture;
                copyTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                RenderTexture.active = activeRT;
            }
            // source is Texture2D
            else if (texture.GetType() == typeof(Texture2D)) 
            {
                copyTexture = new(texture.width, texture.height, ((Texture2D)texture).format, false);
                Graphics.CopyTexture(texture, copyTexture);
            }
            // source is Generic Texture (webcam, etc)
            else
            {
                copyTexture = new(texture.width, texture.height, TextureFormat.RGBA32, false);
                Graphics.CopyTexture(texture, copyTexture);
            }

            // Encode to PNG
            byte[] bytes = ImageConversion.EncodeToPNG(copyTexture);
            UnityEngine.Object.Destroy(copyTexture);

            // Create a file name
            string now = DateTime.Now.ToString("yyyy.MM.dd_hh.mm.ss.fff");

            // Create "Export" directory if necessary, and export PNG
            string exportDir = RayToneController.BASE_DIR + "/Export/";
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }
            File.WriteAllBytes(exportDir + now + ".png", bytes);
            Console.Log("Exported PNG as " + now);
        }
    }
}
