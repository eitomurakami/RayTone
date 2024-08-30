//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(true);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------

// DSP chain
RAYTONE_INPUT(0) => FFT fft =^ Centroid centroid => blackhole;

// Initialize FFT
4096 => fft.size;
Windowing.hann(fft.size()) => fft.window;
fft.size()::samp => dur HOP;
second / samp => float srate;

// infinite loop
while (true)
{
    // Advance time
    HOP => now;

    // Compute centroid
    centroid.upchuck();
    centroid.fval(0) * srate / 2 => RAYTONE_OUTLET;
}
