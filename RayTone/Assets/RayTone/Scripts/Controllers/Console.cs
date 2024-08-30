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
using UnityEngine.UI;

namespace RayTone
{
    public class Console : Singleton<Console>
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Canvas openpanel;
        [SerializeField] private Canvas scrollview;
        [SerializeField] private UnityEngine.UI.Button openButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        private static TMP_Text text_static;
        private static Canvas openpanel_static;
        private static Canvas scrollview_static;     
        private static UnityEngine.UI.Button openButton_static;
        private static UnityEngine.UI.Button closeButton_static;

        private static bool updateText = false;
        private static string textRaw = "";

        /////
        //AWAKE
        protected override void Awake()
        {
            base.Awake();
            text_static = text;

            openpanel_static = openpanel;
            openpanel_static.enabled = true;

            scrollview_static = scrollview;
            scrollview_static.enabled = false;

            openButton_static = openButton;
            closeButton_static = closeButton;
        }

        /////
        //START
        void Start()
        {
            // Register GUI
            openButton_static.onClick.AddListener(delegate { OnOpen(); });
            closeButton_static.onClick.AddListener(delegate { OnClose(); });
        }

        /////
        //UPDATE
        private void Update()
        {
            // Update text
            if (updateText)
            {
                text_static.text = textRaw;
                updateText = false;
            }
        }

        /// <summary>
        /// Print to the console
        /// </summary>
        public static void Log(string log, bool openConsole = false)
        {
            textRaw += log + "\n";
            updateText = true;
            if (openConsole)
            {
                SetConsoleVisibility(true);
            }
        }

        /// <summary>
        /// Set Console visibility
        /// </summary>
        /// <param name="visible"></param>
        public static void SetConsoleVisibility(bool visible)
        {
            openpanel_static.enabled = !visible;
            scrollview_static.enabled = visible;
        }

        /// <summary>
        /// Toggle console visibility
        /// </summary>
        public static void ToggleConsoleVisibility()
        {
            openpanel_static.enabled = !openpanel_static.enabled;
            scrollview_static.enabled = !scrollview_static.enabled;
        }

        /// <summary>
        /// Called by close button
        /// </summary>
        public static void OnOpen()
        {
            SetConsoleVisibility(true);
        }

        /// <summary>
        /// Called by close button
        /// </summary>
        public static void OnClose()
        {
            SetConsoleVisibility(false);
        }
    }
}
