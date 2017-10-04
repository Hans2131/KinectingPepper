#define SINK_EXPORTS
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

#define DEPTH

//active or not
bool active = 0;
// Format constants
#ifdef DEPTH
const UINT32 VIDEO_WIDTH = 512;
const UINT32 VIDEO_HEIGHT = 424;
#else
const UINT32 VIDEO_WIDTH = 1920;
const UINT32 VIDEO_HEIGHT = 1080;
#endif
const UINT32 VIDEO_FPS = 25;
const UINT64 VIDEO_FRAME_DURATION = 10 * 1000 * 1000 / VIDEO_FPS;
const UINT32 VIDEO_BIT_RATE = 800000;
const GUID   VIDEO_ENCODING_FORMAT = MFVideoFormat_H264;
const GUID   VIDEO_INPUT_FORMAT = MFVideoFormat_RGB32;
const UINT32 VIDEO_PELS = VIDEO_WIDTH * VIDEO_HEIGHT;
//const UINT32 VIDEO_FRAME_COUNT = 20 * VIDEO_FPS;

// Buffer to hold the video frame data.
//DWORD videoFrameBuffer[VIDEO_PELS];
DWORD* videoFrameBuffer;
DWORD streamindex;
//sink to send data to
IMFSinkWriter *pSinkWriter = NULL;
//timeindex (increment by video_frame_duration after each frame)
LONGLONG rtStart = 0;
WCHAR* bufferfile;// = L"C://images/Pepper/DepthOut.mp4";
char* deletefile;// = "C://images/Pepper/DepthOut.mp4";

extern "C"{
	extern __declspec(dllexport) int WriteFrame();
	extern __declspec(dllexport) void Shutdown();
	extern __declspec(dllexport) void Init();
	extern __declspec(dllexport) bool IsActive();
//	extern __declspec(dllexport) void SetBuffer(unsigned long*);	
	extern __declspec(dllexport) unsigned long** GetBuffer();
	extern __declspec(dllexport) void SetPath(char[]);
}

template <class T> void  SafeRelease(T **ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}

HRESULT  InitializeSinkWriter(IMFSinkWriter **ppWriter, DWORD *pStreamIndex)
{
	*ppWriter = NULL;
	*pStreamIndex = NULL;

	IMFSinkWriter   *pSinkWriter = NULL;
	IMFMediaType    *pMediaTypeOut = NULL;
	IMFMediaType    *pMediaTypeIn = NULL;
	DWORD           streamIndex;

	remove(deletefile);
	HRESULT hr = MFCreateSinkWriterFromURL(bufferfile, NULL, NULL, &pSinkWriter);

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
		hr = pMediaTypeOut->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
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

int  WriteFrame()
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
			(BYTE*)(videoFrameBuffer),    // First row in source image.
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
		hr = pSample->SetSampleTime(rtStart);
	}
	if (SUCCEEDED(hr))
	{
		hr = pSample->SetSampleDuration(VIDEO_FRAME_DURATION);
	}

	// Send the sample to the Sink Writer.
	if (SUCCEEDED(hr))
	{
		hr = pSinkWriter->WriteSample(streamindex, pSample);
	}
	rtStart += VIDEO_FRAME_DURATION;
	SafeRelease(&pSample);
	SafeRelease(&pBuffer);
	return (int)hr;
}

void  Init()
{
	if (!active) {
		active = true;
		HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		hr = MFStartup(MF_VERSION);
		InitializeSinkWriter(&pSinkWriter, &streamindex);
	}
}

void SetPath(char pathName[]) {
	char mp4[5] = ".mp4";
	int i=0, j=0;
	while ((pathName[i] != 0)&&(mp4[j]!=0)) {
		if (pathName[i] == mp4[j])j++;
		else j = 0;
		i++;
	}
	if (deletefile)delete[] deletefile;
	if (bufferfile)delete[] bufferfile;
	deletefile = new char[i+1];
	bufferfile = new wchar_t[i + 1];
	for (j = 0; j < i; j++) {
		deletefile[j] = pathName[j];
		bufferfile[j] = pathName[j];
	}
	deletefile[j] = 0;
	bufferfile[j] = 0;

}

void  Shutdown() {
	if (active) {
		active = false;
		if (pSinkWriter) {
			pSinkWriter->Finalize();
			SafeRelease(&pSinkWriter);
		}
		MFShutdown();
		CoUninitialize();
		
	}
}

bool IsActive() {
	return active;
}
/*/
void SetBuffer(unsigned long* newbuffer){
	videoFrameBuffer = newbuffer;
}
*/

unsigned long** GetBuffer(){
	return &videoFrameBuffer;
}
