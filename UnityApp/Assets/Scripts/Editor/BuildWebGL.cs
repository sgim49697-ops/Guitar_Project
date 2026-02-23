// BuildWebGL.cs - WebGL 빌드 자동화 Editor 유틸리티
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildWebGL
{
    private const string BuildPath = "Builds/WebGL";
    private const string ScenePath = "Assets/MainScene.unity";

    [MenuItem("Guitar/Build WebGL")]
    public static void Build()
    {
        // MCP 타임아웃 방지: 즉시 반환하고 빌드는 다음 프레임에 예약
        // (BuildPipeline.BuildPlayer가 메인 스레드를 장시간 블로킹하면
        //  MCP WebSocket이 먼저 닫혀 "error in sending data" 발생)
        Debug.Log("[BuildWebGL] 빌드 예약됨 — 다음 프레임에 시작합니다.");
        EditorApplication.delayCall += RunBuild;
    }

    private static void RunBuild()
    {
        string fullPath = Path.GetFullPath(BuildPath);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        Debug.Log($"[BuildWebGL] 빌드 시작 → {fullPath}");

        // Build Settings 씬 등록
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes           = new[] { ScenePath },
            locationPathName = BuildPath,
            target           = BuildTarget.WebGL,
            subtarget        = (int)WebGLTextureSubtarget.Generic,
            options          = BuildOptions.None,
        };

        // WebGL 설정
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.linkerTarget      = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.exceptionSupport  = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.dataCaching       = true;
        PlayerSettings.productName             = "GuitarPractice";

        BuildReport  report  = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildWebGL] ✅ 빌드 성공! 크기: {summary.totalSize / 1024 / 1024} MB, 경로: {fullPath}");
            EditorUtility.RevealInFinder(fullPath);
        }
        else
        {
            Debug.LogError($"[BuildWebGL] ❌ 빌드 실패: {summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"  {msg.content}");
                }
            }
        }
    }
}
