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
    public class Menu_GraphicsEffect : Menu_Unit
    {
        private GraphicsUnit_Effect graphicsUnit_Effect;

        // reference to input text
        [SerializeField] private TMP_InputField resolutionXInput;
        [SerializeField] private TMP_InputField resolutionYInput;
        [SerializeField] private UnityEngine.UI.Toggle filterToggle;

        /////
        //START
        void Start()
        {
            graphicsUnit_Effect = parentUnit.GetComponent<GraphicsUnit_Effect>();

            //Register GUI
            resolutionXInput.SetTextWithoutNotify(graphicsUnit_Effect.GetResolution().x.ToString());
            resolutionXInput.onEndEdit.AddListener(delegate { OnResolutionXChanged(resolutionXInput.text); });

            resolutionYInput.SetTextWithoutNotify(graphicsUnit_Effect.GetResolution().y.ToString());
            resolutionYInput.onEndEdit.AddListener(delegate { OnResolutionYChanged(resolutionYInput.text); });

            filterToggle.SetIsOnWithoutNotify(graphicsUnit_Effect.GetFilterMode() == 1);
            filterToggle.onValueChanged.AddListener(delegate { OnFilterModeChanged(filterToggle.isOn); });
        }

        // Called by ResolutionX text input
        private void OnResolutionXChanged(string val)
        {
            if (int.TryParse(val, out int val_int))
            {
                graphicsUnit_Effect.SetResolution(new Vector2(val_int, graphicsUnit_Effect.GetResolution().y));
            }
        }

        // Called by ResolutionY text input
        private void OnResolutionYChanged(string val)
        {
            if (int.TryParse(val, out int val_int))
            {
                graphicsUnit_Effect.SetResolution(new Vector2(graphicsUnit_Effect.GetResolution().x, val_int));
            }
        }

        // Called by Filter toggle
        private void OnFilterModeChanged(bool arg)
        {
            graphicsUnit_Effect.SetFilterMode(arg ? 1 : 0);
        }
    }
}
