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
using System.Collections.Generic;
using UnityEngine;

namespace RayTone
{
    public class Menu_Softkeys1 : MonoBehaviour
    {
        private RayToneController raytoneController;
        private PlayerController playerController;
        private CameraController cameraController;

        /////
        //START
        private void Start()
        {
            raytoneController = RayToneController.Instance;
            playerController = PlayerController.Instance;
            cameraController = CameraController.Instance;
        }

        // Called by Setting button
        public void OnSettings()
        {
            playerController.OnS();
        }

        // Called by C button
        public void OnC()
        {
            if(cameraController.GetVisibility())
            {
                playerController.OnC();
            }
        }

        // Called by V button
        public void OnV()
        {
            if (cameraController.GetVisibility())
            {
                playerController.OnV();
            }
        }

        // Called by G button
        public void OnG()
        {
            if (cameraController.GetVisibility())
            {
                playerController.OnG();
            }
        }

        // Called by D button
        public void OnD()
        {
            if (cameraController.GetVisibility())
            {
                playerController.OnDuplicate();
            }
        }

        // Called by Delete button
        public void OnDelete()
        {
            if (cameraController.GetVisibility())
            {
                playerController.OnDelete();
            }
        }

        // Called by performance button
        public void OnPerformanceToggle()
        {
            if (cameraController.GetVisibility())
            {
                playerController.OnPerformanceToggle();
            }
        }

        // Called by Edit button
        public void OnEdit()
        {
            if (cameraController.GetVisibility())
            {
                if (!raytoneController.GetEditStatus())
                {
                    raytoneController.EnterEdit();
                }
                else
                {
                    playerController.OnM();
                }
            }
        }

        // Called by Return button
        public void OnReturn()
        {
            if (cameraController.GetVisibility())
            {
                raytoneController.ExitEdit();
            }
        }
    }
}
