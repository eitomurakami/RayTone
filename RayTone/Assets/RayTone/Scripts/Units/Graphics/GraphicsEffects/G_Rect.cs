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
    public class G_Rect : GraphicsUnit_Effect
    {
        /////
        //UPDATE
        private void Update()
        {
            Vector2 center = new(0.5f, 0.5f);
            Vector2 size = new(0.5f, 0.5f);
            float blur = 0f;
            float blurSize = 0f;

            if (GetInletStatus(0)) { center.x = GetInletVal(0); }
            if (GetInletStatus(1)) { center.y = GetInletVal(1); }
            if (GetInletStatus(2)) { size.x = GetInletVal(2); }
            if (GetInletStatus(3)) { size.y = GetInletVal(3); }
            if (GetInletStatus(4)) { blur = GetInletVal(4); }
            if (GetInletStatus(5)) { blurSize = GetInletVal(5); }

            material.SetVector("_Center", center);
            material.SetVector("_Size", size / new Vector2(1920f / 1080f, 1f));
            material.SetFloat("_Blur", blur);
            material.SetFloat("_BlurSize", blurSize);

            // Render frame with custom shader
            Graphics.Blit(null, renderTexture, material, 0);

            // Send render frame request
            NotifyQueueRenderFrame();
        }
    }
}
