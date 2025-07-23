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
    public class G_Transform : GraphicsUnit_Effect
    {
        /// <summary>
        /// Render frame
        /// </summary>
        protected override void RenderFrame()
        {
            if (GetInletStatus(0))
            {
                Vector2 scale = new(1f, 1f);
                float rotate = 0f;
                Vector2 translate = new(0f, 0f);
                Vector2 pivot = new(0.5f, 0.5f);

                if (GetInletStatus(1)) { scale.x = GetInletVal(1); }
                if (GetInletStatus(2)) { scale.y = GetInletVal(2); }
                if (GetInletStatus(3)) { rotate = GetInletVal(3); }
                if (GetInletStatus(4)) { translate.x = GetInletVal(4); }
                if (GetInletStatus(5)) { translate.y = GetInletVal(5); }
                if (GetInletStatus(6)) { pivot.x = GetInletVal(6); }
                if (GetInletStatus(7)) { pivot.y = GetInletVal(7); }

                material.SetVector("_Scale", scale);
                material.SetFloat("_Rotate", rotate);
                material.SetVector("_Translate", translate);
                material.SetVector("_Pivot", pivot);
                material.SetVector("_Resolution", resolution);
            }

            base.RenderFrame();
        }  
    }
}
