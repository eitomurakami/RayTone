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
    public class SelectBox : MonoBehaviour
    {
        RayToneController rayToneController;

        // ///
        //AWAKE
        private void Awake()
        {
            this.gameObject.SetActive(false);
        }

        /////
        //START
        private void Start()
        {
            rayToneController = RayToneController.Instance;
        }

        /// <summary>
        /// Have select box update unit
        /// </summary>
        /// <param name="unit"></param>
        private void OnTriggerEnter(Collider other)
        {
            other.gameObject.TryGetComponent<Unit>(out Unit unit);
            if (unit)
            {
                rayToneController.SelectUnit(unit);
            }
        }

        /// <summary>
        /// Have select box deselect unit
        /// </summary>
        /// <param name="unit"></param>
        private void OnTriggerExit(Collider other)
        {
            other.gameObject.TryGetComponent<Unit>(out Unit unit);
            if (unit)
            {
                rayToneController.DeselectUnit(unit);
            }
        }
    }
}
