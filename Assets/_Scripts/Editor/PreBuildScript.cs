using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PreBuildScript : IPreprocessBuildWithReport {
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report) {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            if (player.TryGetComponent<CharacterDash>(out _)) {
                AddDefineSymbols("DASH_COMPONENT");
            }
            else {
                RemoveDefineSymbols("DASH_COMPONENT");
            }

            if (player.TryGetComponent<CharacterLedgeGrab>(out _)) {
                AddDefineSymbols("LEDGE_GRAB_COMPONENT");
            }
            else {
                RemoveDefineSymbols("LEDGE_GRAB_COMPONENT");
            }

            if (player.TryGetComponent<CharacterSlide>(out _)) {
                AddDefineSymbols("SLIDE_COMPONENT");
            }
            else {
                RemoveDefineSymbols("SLIDE_COMPONENT");
            }
            if (player.TryGetComponent<CharacterCombat>(out _)) {
                AddDefineSymbols("COMBAT_COMPONENT");
            }
            else {
                RemoveDefineSymbols("COMBAT_COMPONENT");
            }
        }
        else {
            Debug.LogWarning("Could not find gameObject with tag: 'Player'. Please make sure your player " +
                "object is tagged as 'Player' in order to exlcude unnecessary code from your build, if needed.");
        }
    }

    private void RemoveDefineSymbols(string symbol) {
        string symbolToRemove = symbol;
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (currentSymbols.Contains(symbolToRemove)) {
            currentSymbols = currentSymbols.Replace(symbolToRemove, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentSymbols);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }

    private void AddDefineSymbols(string symbol) {
        string symbolToAdd = symbol;
        string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (!currentSymbols.Contains(symbolToAdd)) {
            currentSymbols = currentSymbols + ";" + symbolToAdd;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentSymbols);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}