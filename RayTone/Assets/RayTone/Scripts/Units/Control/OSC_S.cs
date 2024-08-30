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
    public class OSC_S : ControlUnit
    {
        OscJack.OscClient client;

        private string ipAddress = "127.0.0.1";
        private string oscAddress = "/raytone/osc-s";
        private int port = 5000;

        /////
        //START
        protected override void Start()
        {
            base.Start();
            ReconnectClient();
        }

        /// <summary>
        /// Override custom update
        /// </summary>
        private void Update()
        {
            if(GetInletStatus(0))
            {
                client.Send(oscAddress, GetInletVal(0));
            }
        }

        /// <summary>
        /// Reconnect OSC Client
        /// </summary>
        private void ReconnectClient()
        {
            client = OscJack.OscMaster.GetSharedClient(ipAddress, port);
        }

        /// <summary>
        /// Set IP Address
        /// </summary>
        /// <param name="ipAddress_arg"></param>
        public void SetIPAddress(string ipAddress_arg)
        {
            ipAddress = ipAddress_arg;
            ReconnectClient();
        }
        /// <summary>
        /// Get IP Address
        /// </summary>
        /// <returns></returns>
        public string GetIPAddress()
        {
            return ipAddress;
        }

        /// <summary>
        /// Set OSC Address
        /// </summary>
        /// <param name="oscAddress_arg"></param>
        public void SetOSCAddress(string oscAddress_arg)
        {
            oscAddress = oscAddress_arg;
            ReconnectClient();
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
            ReconnectClient();
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
            if(up.metaString.ContainsKey("ip_address"))
            {
                ipAddress = up.metaString["ip_address"];
            }
            if (up.metaString.ContainsKey("osc_address"))
            {
                oscAddress = up.metaString["osc_address"];
            }
            if (up.metaInt.ContainsKey("osc_port"))
            {
                port = up.metaInt["osc_port"];
            }
            ReconnectClient();
        }

        /// <summary>
        /// Get unit properties
        /// </summary>
        /// <returns></returns>
        public override UnitProperties GetUnitProperties()
        {
            UnitProperties up = new();

            up.metaString = new();
            up.metaString.Add("ip_address", ipAddress);
            up.metaString.Add("osc_address", oscAddress);

            up.metaInt = new();
            up.metaInt.Add("osc_port", port);

            return up;
        }
    }
}
