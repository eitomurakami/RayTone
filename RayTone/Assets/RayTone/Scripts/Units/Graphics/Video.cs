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
using UnityEngine.Video;

namespace RayTone
{
    public class Video : GraphicsUnit
    {
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private TextMeshProUGUI imageText;

        private bool stepUpdate = false;

        private VideoPlayer videoPlayer;
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
            StartStepListener();

            videoPlayer = GetComponent<VideoPlayer>();
            graphicsController = GraphicsController.Instance;
            material = mesh.material;
            StartCoroutine(GetVideo(GetFilePath()));
        }

        /////
        //UPDATE
        private void Update()
        {
            if (stepUpdate)
            {
                GetInletVal(0); // Force recursive inlet update

                // Play video on trigger
                if (GetInletStatus(0) && inlets[0].connectedUnit.UpdateTrigger() == 1)
                {
                    videoPlayer.Play();
                    videoPlayer.time = GetInletVal(2) * 0.001;  // position in ms
                }
                stepUpdate = false;
            }

            widthMultiplier = 1f;
            heightMultiplier = 1f;

            // playback rate
            if (GetInletStatus(1))
            {
                videoPlayer.playbackSpeed = GetInletVal(1);
            }
            // loop
            if (GetInletStatus(3))
            {
                float inlet3 = GetInletVal(3);
                if (inlet3 > 0)
                {
                    videoPlayer.isLooping = true;
                }
                else
                {
                    videoPlayer.isLooping = false;
                }
            }
            // width
            if (GetInletStatus(4))
            {
                widthMultiplier = GetInletVal(4);
            }
            // height
            if(GetInletStatus(5))
            {
                heightMultiplier = GetInletVal(5);
            }

            // Update resolution
            if (texture)
            {
                graphicsController.SetTextureResolution(texture, width * widthMultiplier, height * heightMultiplier);
            }
            mesh.transform.localScale = new Vector3(width * widthMultiplier * 0.0015f, 0.01f, height * heightMultiplier * 0.0015f);  // sweet scale multiplier...

            // opacity
            if (GetInletStatus(6))
            {
                material.color = new Color(1, 1, 1, GetInletVal(6));
            }
            else
            {
                material.color = new Color(1, 1, 1, 1);
            }

            // Send render frame request
            NotifyQueueRenderFrame();
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            StopStepListener();
            graphicsController.RemoveTexture(texture);
            Destroy(material);
            material = null;
            texture = null;
        }

        // quantized tick
        public override void Step()
        {
            stepUpdate = true;
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
            StartCoroutine(GetVideo(GetFilePath()));
        }

        /// <summary>
        /// Load video
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        IEnumerator GetVideo(string filePath)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        //filePath = filePath.Insert(0, "file://");
#endif
            if (!File.Exists(filePath))
            {
                Console.Log("Video Unit: Failed to load " + Path.GetFileName(filePath) + ".", true);
                yield break;
            };
            
            // Load file
            videoPlayer.url = filePath;
            videoPlayer.Play();
            yield return new WaitUntil(() => videoPlayer.texture);

            // Get video texture reference
            texture = videoPlayer.texture;

            // Assign material
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
