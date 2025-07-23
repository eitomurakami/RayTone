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
using UnityEngine;

namespace RayTone
{
    public class G_Multiply : GraphicsUnit_Effect
    {
        private int texture2Index = -1;

        /// <summary>
        /// Render frame
        /// </summary>
        protected override void RenderFrame()
        {
            if (GetInletStatus(1))
            {
                int inletVal = (int)GetInletVal(1);
                if (texture2Index != inletVal)
                {
                    // Retrieve source texture
                    texture2Index = inletVal;
                    Texture texture = graphicsController.GetTextureWithID(texture2Index);

                    // Update material
                    material.SetTexture("_SecondTex", texture);
                }
            }
            else
            {
                // Reset to null texture
                if (texture2Index != 0)
                {
                    texture2Index = 0;
                    material.SetTexture("_SecondTex", graphicsController.GetTextureWithID(texture2Index));
                }
            }

            base.RenderFrame();
        }
    }
}
