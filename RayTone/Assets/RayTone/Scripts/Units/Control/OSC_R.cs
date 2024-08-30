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

using OscJack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OscJack.OscEventReceiver;

namespace RayTone
{
    public class OSC_R : ControlUnit
    {
        OscJack.OscServer server;

        private string oscAddress = "/raytone/osc-r";
        private int port = 5001;
        private float outVal = 0f;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            ReconnectServer();
        }

        /// <summary>
        /// Reconnect OSC Server
        /// </summary>
        private void ReconnectServer()
        {
            server = OscJack.OscMaster.GetSharedServer(port);
            server.MessageDispatcher.AddCallback(oscAddress, OnDataReceive);
        }

        /// <summary>
        /// Parse data
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        private void OnDataReceive(string address, OscDataHandle data)
        {
            outVal = data.GetElementAsFloat(0);
        }

        /// <summary>
        /// Chained output
        /// </summary>
        /// <returns></returns>
        public override float UpdateOutput()
        {
            StoreValue(outVal);
            return outVal;
        }

        /// <summary>
        /// Set OSC Address
        /// </summary>
        /// <param name="oscAddress_arg"></param>
        public void SetOSCAddress(string oscAddress_arg)
        {
            oscAddress = oscAddress_arg;
            ReconnectServer();
        }

        /// <summary>
        /// Get OSC Address
        /// </summary>
        /// <returns></returns>
        public string GetOSCAddress()
        {
            return oscAddress;
        }

        /// <summary>
        /// Set Port
        /// </summary>
        /// <param name="port_arg"></param>
        public void SetPort(int port_arg)
        {
            port = port_arg;
            ReconnectServer();
        }
        /// <summary>
        /// Get Port
        /// </summary>
        /// <returns></returns>
        public int GetPort()
        {
            return port;
        }

        /// <summary>
        /// Apply unit properties
        /// </summary>
        /// <param name="up"></param>
        public override void ApplyUnitProperties(UnitProperties up)
        {
            if (up.metaString.ContainsKey("osc_address"))
            {
                oscAddress = up.metaString["osc_address"];
            }
            if (up.metaInt.ContainsKey("osc_port"))
            {
                port = up.metaInt["osc_port"];
            }
            ReconnectServer();
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaString = new();
            up.metaString.Add("osc_address", oscAddress);

            up.metaInt = new();
            up.metaInt.Add("osc_port", port);

            return up;
        }
    }
}
