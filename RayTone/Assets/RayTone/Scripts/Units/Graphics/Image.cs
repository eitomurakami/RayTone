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

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.IO;

namespace RayTone
{
    public class Image : GraphicsUnit
    {
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private TextMeshProUGUI imageText;

        private GraphicsController graphicsController;

        private Texture texture;
        private Material material;
        private float width;
        private float height;
        private float widthMultiplier = 1f;
        private float heightMultiplier = 1f;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            material = mesh.material;
            graphicsController = GraphicsController.Instance;
            StartCoroutine(GetTexture(GetFilePath()));
        }

        /////
        //UPDATE
        private void Update()
        {
            widthMultiplier = 1f;
            heightMultiplier = 1f;

            // width
            if (GetInletStatus(0))
            {
                widthMultiplier = GetInletVal(0);
            }
            // height
            if(GetInletStatus(1))
            {
                heightMultiplier = GetInletVal(1);
            }

            // Update resolution
            if (texture)
            {
                graphicsController.SetTextureResolution(texture, width * widthMultiplier, height * heightMultiplier);
            }
            mesh.transform.localScale = new(width * widthMultiplier * 0.0015f, 0.01f, height * heightMultiplier * 0.0015f);  // sweet scale multiplier...

            // opacity
            if (GetInletStatus(2))
            {
                material.color = new(1, 1, 1, GetInletVal(2));
            }
            else
            {
                material.color = new(1, 1, 1, 1);
            }

            // Send render frame request
            NotifyQueueRenderFrame();
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            graphicsController.RemoveTexture(texture);
            Destroy(material);
            material = null;
            texture = null;
        }

        /// <summary>
        /// texture ID
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            int id = 0;
            
            if (texture)
            {
                id = graphicsController.GetTextureID(texture);
            }

            StoreValue(id);
            return id;
        }

        /// <summary>
        /// Reset file path
        /// </summary>
        /// <param name="filePath_arg"></param>
        public override void ReattachFilePath()
        {
            StartCoroutine(GetTexture(GetFilePath()));
        }

        /// <summary>
        /// Load texture
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        IEnumerator GetTexture(string filePath)
        {
            #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        filePath = filePath.Insert(0, "file://");
            #endif

            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Console.Log("Image Unit: Failed to load " + Path.GetFileName(filePath) + ".", true);
            }
            else
            {
                // Set material
                texture = DownloadHandlerTexture.GetContent(uwr);
                texture.wrapMode = TextureWrapMode.Repeat;
                width = texture.width;
                height = texture.height;
                material.mainTexture = texture;
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_EmissionMap", texture);

                // Store texture
                graphicsController.AddTexture(texture);

                // Set text
                imageText.text = Path.GetFileNameWithoutExtension(filePath);
            }
        }
    }
}
