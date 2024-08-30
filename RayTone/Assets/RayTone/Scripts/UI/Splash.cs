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
using TMPro;

namespace RayTone
{
    public class Splash : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI versiontext;
        [SerializeField] private GameObject introtext;

        private float duration = 4f;
        private bool firstTime = false;
        private RayToneController raytoneController;
        /////
        //START
        void Start()
        {
            raytoneController = RayToneController.Instance;
            Invoke("Destroy", duration);
        }

        public void DisplayIntro()
        {
            firstTime = true;
            duration = 20f;
            introtext.SetActive(true);
        }

        /// <summary>
        /// Set text
        /// </summary>
        /// <param name="text_arg"></param>
        public void SetVersionText(string text_arg)
        {
            versiontext.text = text_arg;
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void Destroy()
        {  
            if (firstTime)
            {
                UserConfig config = raytoneController.ReadUserConfig();
                config.firstTime = false;
                RayToneController.Instance.WriteUserConfig(config);
            }
            Destroy(this.gameObject);
        }
    }
}