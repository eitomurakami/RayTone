//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("in1", "in2", "in3", "in4");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
Gain g;
1 => g.gain;
RAYTONE_INPUT(0) => g;
RAYTONE_INPUT(1) => g;
RAYTONE_INPUT(2) => g;
RAYTONE_INPUT(3) => g;
g => RAYTONE_OUTPUT => dac;

// infinite loop
while (true)
{
	1000::ms => now;
}
