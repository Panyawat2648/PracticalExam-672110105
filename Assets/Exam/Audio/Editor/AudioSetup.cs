using UnityEngine;
using UnityEditor;
namespace PanyawatHainak.Editor {
public static class AudioSetup {
    public static void Configure(){
        foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Exam/Audio/Resources"})){
            var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            bool music=path.EndsWith("Menu.wav")||path.EndsWith("Stage1.wav")||path.EndsWith("Stage2.wav");
            var settings=importer.defaultSampleSettings;
            settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=music?.65f:.85f;
            importer.defaultSampleSettings=settings;importer.forceToMono=!music;importer.SaveAndReimport();
        }
        Debug.Log("EXAM_AUDIO_IMPORT_OK");
    }
}
}
