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
    public class Menu_Settings : Menu_Global
    {
        // reference to text and sliders
        [SerializeField] private UnityEngine.UI.Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private UnityEngine.UI.Slider clockSlider;
        [SerializeField] private TextMeshProUGUI clockText;
        [SerializeField] private UnityEngine.UI.Slider resolutionSlider;
        [SerializeField] private TextMeshProUGUI resolutionText;
        [SerializeField] private UnityEngine.UI.Toggle postprocessToggle;
        //public TextMeshProUGUI postprocessText;

        //public Slider antialiasSlider;
        //public TextMeshProUGUI antialiasText;

        [SerializeField] private Button loadButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button rescanButton;
        [SerializeField] private Button directoryButton;
        [SerializeField] private Button displayButton;

        private GraphicsController graphicsController;

        /////
        //START
        protected override void Start()
        {
            base.Start();

            graphicsController = GraphicsController.Instance;

            volumeSlider.SetValueWithoutNotify(UnitController.GetGlobalVolume());
            volumeText.text = "volume: " + Mathf.Floor(UnitController.GetGlobalVolume() * 1000f) / 1000f;
            volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChanged(volumeSlider.value); });

            clockSlider.SetValueWithoutNotify(Clock.GetBPM());
            clockText.text = "BPM: " + Clock.GetBPM();
            clockSlider.onValueChanged.AddListener(delegate { OnClockChanged(clockSlider.value); });

            resolutionSlider.SetValueWithoutNotify(CameraController.GetRenderScaleDivider());
            resolutionText.text = ResolutionText(CameraController.GetRenderScaleDivider());
            resolutionSlider.onValueChanged.AddListener(delegate { OnResolutionChanged(resolutionSlider.value); });

            postprocessToggle.SetIsOnWithoutNotify(CameraController.GetPostProcessingStatus());
            postprocessToggle.onValueChanged.AddListener(delegate { OnPostProcessingChanged(postprocessToggle.isOn); });

            /*
            antialiasSlider.SetValueWithoutNotify(CameraController.GetAntiAliasingFactor());
            antialiasText.text = "Anti-Alias: " + CameraController.GetAntiAliasingFactor();
            antialiasSlider.onValueChanged.AddListener(delegate { OnAntiAliasingChanged(antialiasSlider.value); });
            */

            loadButton.onClick.AddListener(delegate { OnLoad(); });
            saveButton.onClick.AddListener(delegate { OnSave(); });
            rescanButton.onClick.AddListener(delegate { OnRescan(); });
            directoryButton.onClick.AddListener(delegate { OnDirectory(); });
            displayButton.onClick.AddListener(delegate { OnDisplay(); });
        }

        /////
        //ON-DESTROY
        private void OnDestroy()
        {
            UserConfig config = raytoneController.ReadUserConfig();
            config.volume = UnitController.GetGlobalVolume();
            config.bpm = (int)Clock.GetBPM();
            config.resolutionDivider = CameraController.GetRenderScaleDivider();
            config.postProcessing = CameraController.GetPostProcessingStatus();
            raytoneController.WriteUserConfig(config);
        }

        // Return resolution text to display based on renderScaleDivider
        private string ResolutionText(float val)
        {
            switch (val)
            {
                case 1:
                    return "Resolution: ULTRA";
                case 2:
                    return "Resolution: HIGH";
                case 3:
                    return "Resolution: MID";
                case 4:
                    return "Resolution: LOW";
            }

            return "";
        }

        // Called by volume slider 
        public void OnVolumeChanged(float val)
        {
            volumeText.text = "volume: " + Mathf.Floor(val * 1000f) / 1000f;
            UnitController.SetGlobalVolume(val);
        }

        // Called by clock slider
        public void OnClockChanged(float val)
        {
            clockText.text = "BPM: " + val;
            Clock.SetBPM((int)val);
        }

        // Called by resolution slider
        public void OnResolutionChanged(float val)
        {
            resolutionText.text = ResolutionText(val);
            CameraController.SetRenderScaleDivider(val);
            graphicsController.SetRenderingQualityDivider((int)val);
        }

        /*
        // Called by anti alias slider
        public void OnAntiAliasingChanged(float val)
        {
            float factor = 0;
            if(val > 0)
            {
                factor = Mathf.Pow(2, val);
            }
            CameraController.SetAntiAliasingFactor(((int)val));
            antialiasText.text = "Anti-Alias: " + CameraController.GetAntiAliasingFactor();
        }
        */

        // Called by post processing toggle
        public void OnPostProcessingChanged(bool enabled)
        {
            CameraController.SetPostProcessingStatus(enabled);
        }

        // Called by Load button
        public void OnLoad()
        {
            raytoneController.LoadProject();
        }

        // Called by Save button
        public void OnSave()
        {
            raytoneController.SaveProject();
        }

        // Called by Rescan button
        public void OnRescan()
        {
            menuController.CloseMenu();
            unitController.LoadChuckFiles();
            midiController.RescanMIDIPorts();
        }

        // Called by Show directory button
        public void OnDirectory()
        {
            string path = RayToneController.BASE_DIR;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            path = path.Replace(" ", "%20");
            path = path.Insert(0, "file://");
#endif
            Application.OpenURL(path);
        }

        // Called by DualDisplay button
        public void OnDisplay()
        {
            CameraController.ActivateSecondDisplay();
        }
    }
}