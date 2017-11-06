
#include <Windows.h>
#include <mfapi.h>
#include <mfidl.h>
#include <Mfreadwrite.h>
#include <mferror.h>

#include <time.h>
#include <cstdio>

#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfuuid")



extern "C" {
	extern __declspec(dllexport) void RGBShutdown();
	extern __declspec(dllexport) void RGBInit();
	extern __declspec(dllexport) bool RGBIsActive();
	extern __declspec(dllexport) void RGBSetPath(char[]);
	extern __declspec(dllexport) void RGBProcess(char*);

	extern __declspec(dllexport) void DShutdown();
	extern __declspec(dllexport) void DInit();
	extern __declspec(dllexport) bool DIsActive();
	extern __declspec(dllexport) void DSetPath(char[]);
	extern __declspec(dllexport) unsigned long* DGetBuffer();
	extern __declspec(dllexport) int DWriteFrame();
}


//all-round 
//functions
template <class T> void  SafeRelease(T **ppT);

//parameters
const UINT32 VIDEO_WIDTH = 640;
const UINT32 VIDEO_HEIGHT = 480;
const UINT32 VIDEO_FPS = 20;
const UINT64 VIDEO_FRAME_DURATION = 10 * 1000 * 1000 / VIDEO_FPS;
const UINT32 VIDEO_BIT_RATE = 8000000;
const GUID   VIDEO_ENCODING_FORMAT = MFVideoFormat_H264;
const char* extension = ".mpeg";
const GUID   VIDEO_INPUT_FORMAT = MFVideoFormat_RGB32;
const UINT32 VIDEO_PELS = VIDEO_WIDTH * VIDEO_HEIGHT;
bool active = false;


//RGB
//functions
bool RGBIsActive();
void RGBInit();
void RGBShutdown();
int RGBWriteFrame();
void RGBProcess(char* b);
void RGBSetPath(char pathName[]);
HRESULT  RGBInitializeSinkWriter(IMFSinkWriter **ppWriter, DWORD *pStreamIndex);

//parameters
DWORD* RGBvideoFrameBuffer = new DWORD[640 * 480];
DWORD RGBstreamindex;
IMFSinkWriter *RGBSinkWriter = NULL;
LONGLONG RGBrtStart = 0;
WCHAR* RGBbufferfile;
char* RGBdeletefile;
bool runRGB = false;

//Depth
//functions
unsigned long* DGetBuffer();
bool DIsActive();
void DInit();
void DShutdown();
void DSetPath(char pathName[]);
int DWriteFrame();
HRESULT  DInitializeSinkWriter(IMFSinkWriter **ppWriter, DWORD *pStreamIndex);

//parameters
DWORD* DvideoFrameBuffer = new DWORD[640 * 480];
DWORD Dstreamindex;
IMFSinkWriter *DSinkWriter = NULL;
LONGLONG DrtStart = 0;
WCHAR* Dbufferfile;
char* Ddeletefile;
bool runDepth = false;


template <class T> void  SafeRelease(T **ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}


bool RGBIsActive() {
	return runRGB;
}

void RGBProcess(char* buffer2) {
	for (int i = 0, j = 479 * 640 * 3; i < 640 * 480; i++) {
		RGBvideoFrameBuffer[i] = ((unsigned char)buffer2[j] << 16) + ((unsigned char)buffer2[j + 1] << 8) + (unsigned char)buffer2[j + 2];
		j += 3;
		if (j % (640 * 3) == 0)
			j -= 640 * 2 * 3;
	}
	RGBWriteFrame();
}

HRESULT  RGBInitializeSinkWriter(IMFSinkWriter **ppWriter, DWORD *pStreamIndex)
{
	*ppWriter = NULL;
	*pStreamIndex = NULL;

	IMFSinkWriter   *pSinkWriter = NULL;
	IMFMediaType    *pMediaTypeOut = NULL;
	IMFMediaType    *pMediaTypeIn = NULL;
	DWORD           streamIndex;

	remove(RGBdeletefile);
	HRESULT hr = MFCreateSinkWriterFromURL(RGBbufferfile, NULL, NULL, &pSinkWriter);

	// Set the output media type.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeOut);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetGUID(MF_MT_SUBTYPE, VIDEO_ENCODING_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetUINT32(MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetUINT32(MF_MT_INTERLACE_MODE, _MFVideoInterlaceMode::MFVideoInterlace_Progressive);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaTypeOut, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->AddStream(pMediaTypeOut, &streamIndex);
	}

	// Set the input media type.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeIn);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_SUBTYPE, VIDEO_INPUT_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaTypeIn, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->SetInputMediaType(streamIndex, pMediaTypeIn, NULL);
	}

	// Tell the sink writer to start accepting data.
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->BeginWriting();
	}

	// Return the pointer to the caller.
	if (SUCCEEDED(hr))
	{
		*ppWriter = pSinkWriter;
		(*ppWriter)->AddRef();
		*pStreamIndex = streamIndex;
	}

	SafeRelease(&pSinkWriter);
	SafeRelease(&pMediaTypeOut);
	SafeRelease(&pMediaTypeIn);
	return hr;
}

int  RGBWriteFrame()
{
	IMFSample *pSample = NULL;
	IMFMediaBuffer *pBuffer = NULL;

	const LONG cbWidth = 4 * VIDEO_WIDTH;
	const DWORD cbBuffer = cbWidth * VIDEO_HEIGHT;

	BYTE *pData = NULL;

	// Create a new memory buffer.
	HRESULT hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);

	// Lock the buffer and copy the video frame to the buffer.
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->Lock(&pData, NULL, NULL);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFCopyImage(
			pData,                      // Destination buffer.
			cbWidth,                    // Destination stride.
			(BYTE*)(RGBvideoFrameBuffer),    // First row in source image.
			cbWidth,                    // Source stride.
			cbWidth,                    // Image width in bytes.
			VIDEO_HEIGHT                // Image height in pixels.
		);
	}
	if (pBuffer)
	{
		pBuffer->Unlock();
	}

	// Set the data length of the buffer.
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->SetCurrentLength(cbBuffer);
	}

	// Create a media sample and add the buffer to the sample.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateSample(&pSample);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->AddBuffer(pBuffer);
	}

	// Set the time stamp and the duration.
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleTime(RGBrtStart);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleDuration(VIDEO_FRAME_DURATION);
	}

	// Send the sample to the Sink Writer.
	if (SUCCEEDED(hr))
	{
		hr = RGBSinkWriter->WriteSample(RGBstreamindex, pSample);
	}
	RGBrtStart += VIDEO_FRAME_DURATION;
	SafeRelease(&pSample);
	SafeRelease(&pBuffer);
	return (int)hr;
}

void  RGBInit()
{
	if (!active) {
		active = true;
		HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		hr = MFStartup(MF_VERSION);
	}
	if (!runRGB) {
		runRGB = true;
		RGBInitializeSinkWriter(&RGBSinkWriter, &RGBstreamindex);
	}
}

void RGBSetPath(char pathName[]) {	
	int i = 0, j = 0;
	while ((pathName[i] != 0) && (extension[j] != 0)) {
		if (pathName[i] == extension[j])j++;
		else j = 0;
		i++;
	}
	if (RGBdeletefile)delete[] RGBdeletefile;
	if (RGBbufferfile)delete[] RGBbufferfile;
	RGBdeletefile = new char[i + 1];
	RGBbufferfile = new wchar_t[i + 1];
	for (j = 0; j < i; j++) {
		RGBdeletefile[j] = pathName[j];
		RGBbufferfile[j] = pathName[j];
	}
	RGBdeletefile[j] = 0;
	RGBbufferfile[j] = 0;

}

void  RGBShutdown() {
	runRGB = false;
	if (RGBSinkWriter) {
		RGBSinkWriter->Finalize();
		SafeRelease(&RGBSinkWriter);
		RGBSinkWriter = nullptr;
		RGBrtStart = 0;
	}
	if ((!runRGB) && (!runDepth) && (active)) {
		active = false;
		MFShutdown();
		CoUninitialize();
	}
}


bool DIsActive() {
	return runDepth;
}

unsigned long* DGetBuffer() {
	return DvideoFrameBuffer;
}

HRESULT  DInitializeSinkWriter(IMFSinkWriter **ppWriter, DWORD *pStreamIndex)
{
	*ppWriter = NULL;
	*pStreamIndex = NULL;

	IMFSinkWriter   *pSinkWriter = NULL;
	IMFMediaType    *pMediaTypeOut = NULL;
	IMFMediaType    *pMediaTypeIn = NULL;
	DWORD           streamIndex;

	remove(Ddeletefile);
	HRESULT hr = MFCreateSinkWriterFromURL(Dbufferfile, NULL, NULL, &pSinkWriter);

	// Set the output media type.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeOut);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetGUID(MF_MT_SUBTYPE, VIDEO_ENCODING_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetUINT32(MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeOut->SetUINT32(MF_MT_INTERLACE_MODE, _MFVideoInterlaceMode::MFVideoInterlace_Progressive);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaTypeOut, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->AddStream(pMediaTypeOut, &streamIndex);
	}

	// Set the input media type.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateMediaType(&pMediaTypeIn);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetGUID(MF_MT_SUBTYPE, VIDEO_INPUT_FORMAT);
	}
	if (SUCCEEDED(hr))
	{
		hr = pMediaTypeIn->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeSize(pMediaTypeIn, MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_FRAME_RATE, VIDEO_FPS, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->SetInputMediaType(streamIndex, pMediaTypeIn, NULL);
	}

	// Tell the sink writer to start accepting data.
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->BeginWriting();
	}

	// Return the pointer to the caller.
	if (SUCCEEDED(hr))
	{
		*ppWriter = pSinkWriter;
		(*ppWriter)->AddRef();
		*pStreamIndex = streamIndex;
	}

	SafeRelease(&pSinkWriter);
	SafeRelease(&pMediaTypeOut);
	SafeRelease(&pMediaTypeIn);
	return hr;
}

int  DWriteFrame()
{
	IMFSample *pSample = NULL;
	IMFMediaBuffer *pBuffer = NULL;

	const LONG cbWidth = 4 * VIDEO_WIDTH;
	const DWORD cbBuffer = cbWidth * VIDEO_HEIGHT;

	BYTE *pData = NULL;

	// Create a new memory buffer.
	HRESULT hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);

	// Lock the buffer and copy the video frame to the buffer.
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->Lock(&pData, NULL, NULL);
	}
	if (SUCCEEDED(hr))
	{
		hr = MFCopyImage(
			pData,                      // Destination buffer.
			cbWidth,                    // Destination stride.
			(BYTE*)(DvideoFrameBuffer),    // First row in source image.
			cbWidth,                    // Source stride.
			cbWidth,                    // Image width in bytes.
			VIDEO_HEIGHT                // Image height in pixels.
		);
	}
	if (pBuffer)
	{
		pBuffer->Unlock();
	}

	// Set the data length of the buffer.
	if (SUCCEEDED(hr))
	{
		hr = pBuffer->SetCurrentLength(cbBuffer);
	}

	// Create a media sample and add the buffer to the sample.
	if (SUCCEEDED(hr))
	{
		hr = MFCreateSample(&pSample);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->AddBuffer(pBuffer);
	}

	// Set the time stamp and the duration.
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleTime(DrtStart);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleDuration(VIDEO_FRAME_DURATION);
	}

	// Send the sample to the Sink Writer.
	if (SUCCEEDED(hr))
	{
		hr = DSinkWriter->WriteSample(Dstreamindex, pSample);
	}
	DrtStart += VIDEO_FRAME_DURATION;
	SafeRelease(&pSample);
	SafeRelease(&pBuffer);
	return (int)hr;
}

void  DInit()
{
	if (!active) {
		active = true;
		HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		hr = MFStartup(MF_VERSION);
	}
	if (!runDepth) {
		runDepth = true;
		DInitializeSinkWriter(&DSinkWriter, &Dstreamindex);
	}
}

void DSetPath(char pathName[]) {
	char mp4[6] = ".mpeg";
	int i = 0, j = 0;
	while ((pathName[i] != 0) && (mp4[j] != 0)) {
		if (pathName[i] == mp4[j])j++;
		else j = 0;
		i++;
	}
	if (Ddeletefile)delete[] Ddeletefile;
	if (Dbufferfile)delete[] Dbufferfile;
	Ddeletefile = new char[i + 1];
	Dbufferfile = new wchar_t[i + 1];
	for (j = 0; j < i; j++) {
		Ddeletefile[j] = pathName[j];
		Dbufferfile[j] = pathName[j];
	}
	Ddeletefile[j] = 0;
	Dbufferfile[j] = 0;

}

void  DShutdown() {
	runDepth = false;
	if (DSinkWriter) {
		DSinkWriter->Finalize();
		SafeRelease(&DSinkWriter);
		DSinkWriter = nullptr;
		DrtStart = 0;
	}
	if ((!runRGB) && (!runDepth) && (active)) {
		active = false;
		MFShutdown();
		CoUninitialize();
	}
}
