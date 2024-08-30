//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("in1", "in2");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(false);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => Gain g1 => RAYTONE_OUTPUT => dac;
RAYTONE_INPUT(1) => Gain g2 => blackhole;

// infinite loop
while(true)
{
	g2.last() => g1.gain;
    1::samp => now;
}
