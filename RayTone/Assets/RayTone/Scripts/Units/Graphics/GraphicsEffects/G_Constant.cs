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
    public class G_Constant : GraphicsUnit_Effect
    {
        /////
        //START
        protected override void Start()
        {
            resolution = new(1, 1);
            base.Start();
        }

        /////
        //UPDATE
        private void Update()
        {
            Vector3 rgb = Vector3.zero;

            if (GetInletStatus(0)) { rgb.x = GetInletVal(0); }
            if (GetInletStatus(1)) { rgb.y = GetInletVal(1); }
            if (GetInletStatus(2)) { rgb.z = GetInletVal(2); }

            material.SetVector("_RGB", rgb);

            // Render frame with custom shader
            Graphics.Blit(null, renderTexture, material, 0);

            // Send render frame request
            NotifyQueueRenderFrame();
        }
    }
}
