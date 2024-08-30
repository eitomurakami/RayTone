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
    public class SelectTextButton : MonoBehaviour
    {
        [System.NonSerialized] public int index = 0;
        [SerializeField] private TextMeshProUGUI buttontext;

        private MenuController menuController;

        private string text = "";

        /////
        //START
        private void Start()
        {
            menuController = MenuController.Instance;
        }

        //Called by button component on click
        public void OnClick()
        {
            menuController.OnSelectTextButtonClick(text);
        }

        /// <summary>
        /// Set text
        /// </summary>
        /// <param name="text_arg"></param>
        public void SetText(string text_arg)
        {
            text = text_arg;

            buttontext.text = text;
            // Remove namespace "RayTone" or directory name if necessary
            if (text.Contains("."))
            {
                buttontext.text = text.Split(".")[1];
            }
            if (text.Contains("/"))
            {
                buttontext.text = text.Split("/")[1];
            }
        }
    }
}
