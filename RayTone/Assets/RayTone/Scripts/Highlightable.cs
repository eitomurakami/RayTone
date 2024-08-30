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
    public class Highlightable : MonoBehaviour
    {
        // prefab reference
        [SerializeField] private GameObject Highlight_PF;
        private GameObject highlight;

        /// <summary>
        /// Highlight
        /// </summary>
        public void Highlight()
        {
            if (highlight != null)
            {
                Dehighlight();
            }

            if(Highlight_PF != null)
            {
                highlight = Instantiate(Highlight_PF);
                highlight.transform.parent = this.transform;
                highlight.transform.localPosition = new Vector3(0f, 0f, 0f);
                highlight.transform.localScale = transform.localScale;
            }
        }

        /// <summary>
        /// De-highlight
        /// </summary>
        public void Dehighlight()
        {
            if (highlight != null)
            {
                Destroy(highlight);
                highlight = null;
            }
        }
    }
}
