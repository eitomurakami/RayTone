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
    public class Text : GraphicsUnit
    { 
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private RectTransform textAreaTransform;
        [SerializeField] private TMP_Text text;

        private RectTransform canvasTransform;
        private RectTransform inputTransform;

        private CameraController cameraController;
        private PlayerController playerController;

        private string currentText = "text";
        private float scaleMultiplier = 1.5f;  // magic number...

        /////
        //START
        protected override void Start()
        {
            base.Start();
            cameraController = CameraController.Instance;
            playerController = PlayerController.Instance;

            canvasTransform = canvas.GetComponent<RectTransform>();
            inputTransform = input.GetComponent<RectTransform>();

            input.onSelect.AddListener(delegate { BeginEdit(); });
            input.onValueChanged.AddListener(delegate { OnEdit(); });
            input.onEndEdit.AddListener(delegate { EndEdit(); });

            AdjustScale();
        }

        /////
        //UPDATE
        private void Update()
        {
            // size
            if (GetInletStatus(0))
            {
                float scale = GetInletVal(0) * scaleMultiplier;
                canvas.transform.localScale = new(scale, scale, scale);
            }
            else
            {
                canvas.transform.localScale = new(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            }

            // opacity
            if (GetInletStatus(1))
            {
                text.color = new Color32(255, 255, 255, (byte)(GetInletVal(1) * 255f));
            }
            else
            {
                text.color = new Color32(255, 255, 255, 255);
            }
        }

        /// <summary>
        /// Enter text edit
        /// </summary>
        public override void OnEnterEdit()
        {
            base.OnEnterEdit();
            input.Select();
        }

        /// <summary>
        /// Begin text editing
        /// </summary>
        private void BeginEdit()
        {
            cameraController.SetMobility(false);
            playerController.DisableInput();
        }

        /// <summary>
        /// Update canvas size based on text length
        /// </summary>
        private void OnEdit()
        {
            GetCurrentText();
            AdjustScale();
        }    

        /// <summary>
        /// End text editing
        /// </summary>
        private void EndEdit()
        {
            cameraController.SetMobility(true);
            playerController.EnableInput();
        }

        /// <summary>
        /// Get latest text
        /// </summary>
        /// <param name="text"></param>
        private void GetCurrentText()
        {
            currentText = input.text;
        }

        /// <summary>
        /// Dynamically adjust canvas scale based on text length
        /// </summary>
        private void AdjustScale()
        {
            float width = System.Math.Clamp(currentText.Length, 1, 20);
            float height = input.textComponent.textInfo.lineCount + 2;
            if (width < 20) height = 2;

            canvasTransform.sizeDelta = new Vector2(width, height * 1f);
            inputTransform.sizeDelta = new Vector2(width, height * 1f);
            textAreaTransform.sizeDelta = new Vector2(width * 10f, height * 10f);
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            input.text = up.metaString["text"];
            GetCurrentText();
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();
            up.metaString = new();
            up.metaString.Add("file", GetFilePath());
            up.metaString.Add("text", input.text);
            return up;
        }
    }
}
