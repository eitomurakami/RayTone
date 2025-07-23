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
    public class G_FBM : GraphicsUnit_Effect
    {
        /////
        //UPDATE
        private void Update()
        {
            Vector2 offset = new(0f, 0f);
            Vector2 scale = new(1f, 1f);
            int octaves = 1;

            if (GetInletStatus(0)) { offset.x = GetInletVal(0); }
            if (GetInletStatus(1)) { offset.y = GetInletVal(1); }
            if (GetInletStatus(2)) { scale.x = GetInletVal(2); }
            if (GetInletStatus(3)) { scale.y = GetInletVal(3); }
            if (GetInletStatus(4)) { octaves = (int)GetInletVal(4); }

            material.SetVector("_Offset", offset);
            material.SetVector("_Scale", scale);
            material.SetFloat("_Octaves", octaves);

            // Render frame with custom shader
            Graphics.Blit(null, renderTexture, material, 0);

            // Send render frame request
            NotifyQueueRenderFrame();
        }
    }
}
