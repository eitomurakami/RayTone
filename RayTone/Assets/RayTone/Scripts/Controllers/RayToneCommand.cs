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
    // Parent RayToneCommand class
    public class RayToneCommand
    {
        protected UnitController unitController;

        public void Init(UnitController unitController_arg)
        {
            unitController = unitController_arg;
        }
        public virtual void Undo() { }
        public virtual void Redo() { }
    }

    // SpawnCommand class
    public class SpawnCommand : RayToneCommand
    {
        RayToneProject project;

        public void Store(RayToneProject project_arg)
        {
            project = project_arg;
        }
        public override void Undo()
        {
            List<Unit> undoneUnits = new();
            RayToneProject projectTemp = new();

            if(project.unit_properties != null)
            {
                // Get current state of units that are about to be undone
                foreach (UnitProperties up in project.unit_properties)
                {
                    undoneUnits.Add(unitController.units[(up.type, up.id)]); 
                }
                projectTemp = unitController.GetProjectData(undoneUnits);

                // Destroy
                foreach (UnitProperties up in project.unit_properties)
                {
                    switch (up.type)
                    {
                        case UnitType.Control:
                            unitController.DestroyControl(unitController.controls[up.id]);
                            break;
                        case UnitType.Voice:
                            unitController.DestroyVoice(unitController.voices[up.id]);
                            break;
                        case UnitType.Graphics:
                            unitController.DestroyGraphics(unitController.graphics[up.id]);
                            break;
                    }
                }

                project = projectTemp;
            }
        }
        public override void Redo()
        {
            unitController.AddProject(project, default, true);
        }
    }

    // DestroyCommand class
    public class DestroyCommand : RayToneCommand
    {
        RayToneProject project;

        public void Store(RayToneProject project_arg)
        {
            project = project_arg;
        }
        public override void Undo()
        {
            unitController.AddProject(project, default, true);
        }
        public override void Redo()
        {
            if (project.unit_properties != null)
            {
                foreach (UnitProperties up in project.unit_properties)
                {
                    if (!up.relUnit)
                    {
                        switch (up.type)
                        {
                            case UnitType.Control:
                                unitController.DestroyControl(unitController.controls[up.id]);
                                break;
                            case UnitType.Voice:
                                unitController.DestroyVoice(unitController.voices[up.id]);
                                break;
                            case UnitType.Graphics:
                                unitController.DestroyGraphics(unitController.graphics[up.id]);
                                break;
                        }
                    }
                }
            }
        }
    }

    // ConnectCommand class
    public class ConnectCommand : RayToneCommand
    {
        (UnitType, int) fromTypeID;
        (UnitType, int) toTypeID;
        int inletIndex;

        public void Store((UnitType, int) fromTypeID_arg, (UnitType, int) toTypeID_arg, int inletIndex_arg)
        {
            fromTypeID = fromTypeID_arg;
            toTypeID = toTypeID_arg;
            inletIndex = inletIndex_arg;
        }
        public override void Undo()
        {
            InletSocket inlet = unitController.units[toTypeID].inlets[inletIndex];
            OutletSocket outlet = unitController.units[fromTypeID].outlet;
            outlet.Disconnect(inlet);
        }
        public override void Redo()
        {
            InletSocket inlet = unitController.units[toTypeID].inlets[inletIndex];
            OutletSocket outlet = unitController.units[fromTypeID].outlet;
            outlet.Connect(inlet, RayToneController.SpawnCable(outlet.transform));
        }
    }

    // ConnectSignalCommand class
    public class ConnectSignalCommand : RayToneCommand
    {
        (UnitType, int) fromTypeID;
        (UnitType, int) toTypeID;
        int inputIndex;

        public void Store((UnitType, int) fromTypeID_arg, (UnitType, int) toTypeID_arg, int inputIndex_arg)
        {
            fromTypeID = fromTypeID_arg;
            toTypeID = toTypeID_arg;
            inputIndex = inputIndex_arg;
        }
        public override void Undo()
        {
            InputSocket input = unitController.units[toTypeID].inputs[inputIndex];
            OutputSocket output = unitController.units[fromTypeID].output;
            output.Disconnect(input);
        }
        public override void Redo()
        {
            InputSocket input = unitController.units[toTypeID].inputs[inputIndex];
            OutputSocket output = unitController.units[fromTypeID].output;
            output.Connect(input, RayToneController.SpawnCable(output.transform));
        }
    }

    // DisconnectCommand class
    public class DisconnectCommand : RayToneCommand
    {
        (UnitType, int) fromTypeID;
        (UnitType, int) toTypeID;
        int inletIndex;

        public void Store((UnitType, int) fromTypeID_arg, (UnitType, int) toTypeID_arg, int inletIndex_arg)
        {
            fromTypeID = fromTypeID_arg;
            toTypeID = toTypeID_arg;
            inletIndex = inletIndex_arg;
        }
        public override void Undo()
        {
            InletSocket inlet = unitController.units[toTypeID].inlets[inletIndex];
            OutletSocket outlet = unitController.units[fromTypeID].outlet;
            outlet.Connect(inlet, RayToneController.SpawnCable(outlet.transform));
        }
        public override void Redo()
        {
            InletSocket inlet = unitController.units[toTypeID].inlets[inletIndex];
            OutletSocket outlet = unitController.units[fromTypeID].outlet;
            outlet.Disconnect(inlet);
        }
    }

    // DisconnectSignalCommand class
    public class DisconnectSignalCommand : RayToneCommand
    {
        (UnitType, int) fromTypeID;
        (UnitType, int) toTypeID;
        int inputIndex;

        public void Store((UnitType, int) fromTypeID_arg, (UnitType, int) toTypeID_arg, int inputIndex_arg)
        {
            fromTypeID = fromTypeID_arg;
            toTypeID = toTypeID_arg;
            inputIndex = inputIndex_arg;
        }
        public override void Undo()
        {
            InputSocket input = unitController.units[toTypeID].inputs[inputIndex];
            OutputSocket output = unitController.units[fromTypeID].output;
            output.Connect(input, RayToneController.SpawnCable(output.transform));
        }
        public override void Redo()
        {
            InputSocket input = unitController.units[toTypeID].inputs[inputIndex];
            OutputSocket output = unitController.units[fromTypeID].output;
            output.Disconnect(input);
        }
    }

    // MoveCommand class
    public class MoveCommand : RayToneCommand
    {
        List<(UnitType, int)> typeIDs;
        Vector3 fromLocation;
        Vector3 toLocation;

        public void Store(List<(UnitType, int)> typeIDs_arg, Vector3 fromLocation_arg = default, Vector3 toLocation_arg = default)
        {
            typeIDs = typeIDs_arg;
            fromLocation = fromLocation_arg;
            toLocation = toLocation_arg;
        }
        public override void Undo()
        {
            if(typeIDs.Count > 0)
            {
                Vector3 offset = default;
                for(int i = 0; i < typeIDs.Count; i++)
                {
                    if(i == 0)
                    {
                        offset = fromLocation - unitController.units[typeIDs[0]].transform.position;
                    }

                    unitController.units[typeIDs[i]].transform.position += offset;
                }
            }
        }
        public override void Redo()
        {
            if (typeIDs.Count > 0)
            {
                Vector3 offset = default;
                for (int i = 0; i < typeIDs.Count; i++)
                {
                    if (i == 0)
                    {
                        offset = toLocation - unitController.units[typeIDs[0]].transform.position;
                    }

                    unitController.units[typeIDs[i]].transform.position += offset;
                }
            }
        }
    }
}
