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
    public class G_Brightness : GraphicsUnit_Effect
    {
        /// <summary>
        /// Render frame
        /// </summary>
        protected override void RenderFrame()
        {
            if (GetInletStatus(0))
            {
                if (GetInletStatus(1))
                {
                    material.SetFloat("_Brightness", GetInletVal(1));
                }
                else
                {
                    material.SetFloat("_Brightness", 1f);
                }
            }

            base.RenderFrame();
        }
    }
}
