#include "TFLiteEngine.h"

#ifndef WHISPER_EXPORTS
#if (defined _WIN32 || defined WINCE || defined __CYGWIN__)
#define WHISPER_EXPORTS __declspec(dllexport)
#elif defined __GNUC__ && __GNUC__ >= 4 && defined(__APPLE__)
#define WHISPER_EXPORTS __attribute__((visibility("default")))
#endif
#endif

#ifndef WHISPER_EXPORTS
#define WHISPER_EXPORTS
#endif

extern "C" {

    // Function to create an instance of TFLiteEngine
    WHISPER_EXPORTS TFLiteEngine* createTFLiteEngine() {
        return new TFLiteEngine();
    }

    // Function to load the model
    WHISPER_EXPORTS int loadModel(TFLiteEngine* engine, const char* modelPath, bool isMultilingual) {
        return engine->loadModel(modelPath, isMultilingual);
    }

    // Function to free the model
    WHISPER_EXPORTS void freeModel(TFLiteEngine* engine) {
        engine->freeModel();
        delete engine;
    }

    // Function to transcribe audio buffer
    WHISPER_EXPORTS const char* transcribeBuffer(TFLiteEngine* engine, const float* samples, int length) {
        std::vector<float> sampleVector(samples, samples + length);
        std::string result = engine->transcribeBuffer(sampleVector);
        char* output = new char[result.size() + 1];
        strcpy(output, result.c_str());
        return output;
    }

    // Function to transcribe audio file
    WHISPER_EXPORTS const char* transcribeFile(TFLiteEngine* engine, const char* waveFile) {
        std::string result = engine->transcribeFile(waveFile);
        char* output = new char[result.size() + 1];
        strcpy(output, result.c_str());
        return output;
    }

} // extern "C"
