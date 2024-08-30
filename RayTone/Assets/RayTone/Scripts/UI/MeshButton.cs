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
using UnityEngine.EventSystems;
using TMPro;

namespace RayTone
{
    public class MeshButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Renderer[] renderers;

        private Color colorHighlight = new(249f / 255f, 253f / 255f, 0f / 255f);
        private Color colorBase = new(220f / 255f, 220f / 255f, 230f / 255f);

        /////
        //START
        void Start()
        {
            Dim();
        }

        /// <summary>
        /// Highlight
        /// </summary>
        private void Highlight()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetColor("_Color", colorHighlight * 1.25f);
            }
        }

        /// <summary>
        /// Dim (base)
        /// </summary>
        private void Dim()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetColor("_Color", colorBase * 1.25f);
            }
        }

        /// <summary>
        /// Triggered on pointer enter
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            Highlight();
        }

        /// <summary>
        /// Triggered on pointer exit
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            Dim();
        }

        /// <summary>
        /// Triggered on click
        /// </summary>
        public void OnClick()
        {
            CancelInvoke();
            Highlight();
            Invoke("Dim", 0.15f);
        }
    }
}
