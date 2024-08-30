//-----------------------------------------------------------------------------
// Define RayTone inputs, inlets, outlet, and file requirement
RAYTONE_DEFINE_INPUTS("signal in");
RAYTONE_DEFINE_INLETS();
RAYTONE_DEFINE_OUTLET(true);
RAYTONE_LOADFILE(false);
//-----------------------------------------------------------------------------
    
// DSP chain
RAYTONE_INPUT(0) => FFT fft =^ RMS rms => blackhole;

// Initialize FFT
1024 => fft.size;
Windowing.hann(1024) => fft.window;

// infinite loop
while (true)
{
    // upchuck: take FFT then RMS
    rms.upchuck() @=> UAnaBlob blob;
    blob.fval(0) => RAYTONE_OUTLET;

    // Advance time 
    fft.size()::samp => now;
}
