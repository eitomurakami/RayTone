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
    public class VFX : GraphicsUnit
    {
        private GraphicsController graphicsController;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            graphicsController = GraphicsController.Instance;
            graphicsController.SetFragmentShaderFilePath(GetFilePath());
            graphicsController.SetRenderingStatus(true);
        }

        /////
        //ON DESTROY
        protected virtual void OnDestroy()
        {
            graphicsController.SetRenderingStatus(false);
        }

        /////
        //UPDATE
        protected virtual void Update()
        {
            for (int i = 0; i < inlets.Length; i++)
            {
                graphicsController.SetInlet(i, GetInletVal(i));
            }
        }

        /// <summary>
        /// Reset file path
        /// </summary>
        /// <param name="filePath_arg"></param>
        public override void ReattachFilePath()
        {
            graphicsController.SetFragmentShaderFilePath(GetFilePath());
        }

        /// <summary>
        /// Override OnEnterEdit
        /// </summary>
        /// <returns></returns>
        public override void OnEnterEdit()
        {
            string file = GetFilePath();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            file = file.Replace(" ", "%20");
            file = file.Insert(0, "file://");
#endif
            Application.OpenURL(file);
        }
    }
}
