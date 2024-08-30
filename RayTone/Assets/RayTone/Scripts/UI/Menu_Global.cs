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
    public class Menu_Global : MonoBehaviour
    {
        // prefab + component references
        [SerializeField] private SelectTextButton SelectTextButton_PF;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private MeshRenderer categoryMeshLeft;
        [SerializeField] private Button categoryButtonLeft;
        [SerializeField] private MeshRenderer categoryMeshRight;
        [SerializeField] private Button categoryButtonRight;
        [SerializeField] private Button pageButtonLeft;
        [SerializeField] private MeshRenderer pageMeshLeft;
        [SerializeField] private Button pageButtonRight;
        [SerializeField] private MeshRenderer pageMeshRight;

        protected RayToneController raytoneController;
        protected UnitController unitController;
        protected MenuController menuController;
        protected MIDIController midiController;

        // private variables
        private string[] categories;
        private int categoryIndex = 0;
        Dictionary<int, List<string>> items;
        private int pageIndex = 0;
        private List<SelectTextButton> buttonsCurrent = new();

        /////
        //START
        protected virtual void Start()
        {
            if (categoryButtonLeft) categoryButtonLeft.onClick.AddListener(delegate { OnCategoryChange(-1); });
            if (categoryButtonRight) categoryButtonRight.onClick.AddListener(delegate { OnCategoryChange(1); });

            if (pageButtonLeft) pageButtonLeft.onClick.AddListener(delegate { OnPageChange(-1); });
            if (pageButtonRight) pageButtonRight.onClick.AddListener(delegate { OnPageChange(1); });

            raytoneController = RayToneController.Instance;
            unitController = UnitController.Instance;
            menuController = MenuController.Instance;
            midiController = MIDIController.Instance;

            UpdateScale();
        }

        /////
        //UPDATE
        private void Update()
        {
            UpdateScale();
        }

        // Called by "x"
        public void ClosePanel()
        {
            if (menuController != null)
            {
                menuController.CloseMenu();
            }
        }

        // Define categories
        public void SetCategories(string[] categories_arg)
        {
            categories = categories_arg;
            categoryIndex = 0;
            UpdateCategory();
        }

        // Define items
        public void SetItems(Dictionary<int, List<string>> items_arg)
        {
            items = items_arg;
            pageIndex = 0;
            UpdatePage();
        }

        // Update category text
        private void UpdateCategory()
        {
            if (categories != null && categories[categoryIndex] != null)
            {
                categoryText.text = categories[categoryIndex];
            }

            // arrow visibility
            if (categoryIndex == 0)
            {
                categoryMeshLeft.enabled = false;
            }
            else
            {
                categoryMeshLeft.enabled = true;
            }
            if(categoryIndex == categories.Length - 1 || categories.Length == 0)
            {
                categoryMeshRight.enabled = false;
            }
            else
            {
                categoryMeshRight.enabled = true;
            }
        }

        // Update page items
        private void UpdatePage()
        {
            foreach(SelectTextButton button_temp in buttonsCurrent)
            {
                Destroy(button_temp.gameObject);
            }
            buttonsCurrent.Clear();

            if(items[categoryIndex] == null)
            {
                return;
            }

            for (int i = 0; i < 16; i++)
            {
                int index = i + 16 * pageIndex; 

                if(index < items[categoryIndex].Count)
                {
                    SelectTextButton button = SpawnButtonInGridWithIndex(i, index);
                    buttonsCurrent.Add(button);

                    if (items[categoryIndex][i] != null)
                    {
                        button.SetText(items[categoryIndex][index]);
                    }
                }
            }

            // arrow visibility
            if (pageIndex == 0)
            {
                pageMeshLeft.enabled = false;
            }
            else
            {
                pageMeshLeft.enabled = true;
            }
            if (pageIndex == items[categoryIndex].Count / 16)
            {
                pageMeshRight.enabled = false;
            }
            else
            {
                pageMeshRight.enabled = true;
            }
        }

        // Called by category arrows
        private void OnCategoryChange(int delta)
        {
            if (categories == null)
            {
                categoryIndex = 0;
                return;
            }
            categoryIndex = Mathf.Clamp(categoryIndex + delta, 0, categories.Length - 1);
            UpdateCategory();
            pageIndex = 0;
            UpdatePage();
        }

        // Called by page arrows
        private void OnPageChange(int delta)
        {
            if(items[categoryIndex] == null)
            {
                return;
            }

            pageIndex = Mathf.Clamp(pageIndex + delta, 0, items[categoryIndex].Count / 16);
            UpdatePage();
        }
        /// <summary>
        /// Spawn button in grid with index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private SelectTextButton SpawnButtonInGridWithIndex(int indexLocal, int indexGlobal)
        {
            float x = -3f + 2f * (indexLocal % 4);
            float y = 1.5f - (indexLocal / 4);
            SelectTextButton button = Instantiate(SelectTextButton_PF);
            button.transform.parent = this.transform;
            button.transform.localRotation = Quaternion.Euler(0, 0, 0);
            button.transform.localPosition = new Vector3(x, y, -0.1f);
            button.transform.localScale = Vector3.one;
            button.index = indexGlobal;

            return button;
        }

        /// <summary>
        /// Adaptive scaling based on screen ratio
        /// </summary>
        private void UpdateScale()
        {
            float scale = Mathf.Clamp(((Screen.width / (float)Screen.height) / 1.78f) * 1.3f, 0.1f, 1.0f);  // adaptive scaling using 16:9 * 1.3 as default
            this.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
