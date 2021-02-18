using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public Toggle isUnloadUnusedAssets;
    public Toggle isGCCollect;
    public Toggle isUnloadRelease;
    private Skybox _skybox;
    private AssetBundle assetBundle;
    private AsyncOperationHandle<Material> addressableLoad;
    private int index;
    private string onGUIText = String.Empty;
    public Text loading;

    private void Start() {
        _skybox = Camera.main.GetComponent<Skybox>();
    }

    private void OnGUI() {
        GUILayout.TextArea(onGUIText);
    }

    public void ToNextSkyBox() {
        if(index > 5) index = 0;
        var path = "skybox" + index;
        var material = Resources.Load<Material>(path);
        _skybox.material = material;
        index++;
        ToReleaseMemory(isUnloadUnusedAssets.isOn, isGCCollect.isOn);
    }
    
    public void ToNextSkyBoxBySteamingAssets() {
        if(assetBundle != null && isUnloadRelease.isOn) {
            assetBundle.Unload(true);
            print("assetBundle.Unload(true)");
        }
        if(index > 5) index = 0;
        var path =  Path.Combine(Application.streamingAssetsPath, "skybox" + index);
        assetBundle = AssetBundle.LoadFromFile(path);
        var material = assetBundle.LoadAsset<Material>("skybox" + index);
        _skybox.material  = material;
        index++;
        StartCoroutine(wait2second());
        assetBundle.Unload(false);
        ToReleaseMemory(isUnloadUnusedAssets.isOn, isGCCollect.isOn);
    }
    
    public void ToNextSkyBoxByAddressableAssets() {
        if(index > 5) index = 0;
        addressableLoad = Addressables.LoadAssetAsync<Material>("skybox" + index);
        StartCoroutine(OnLoad());
        addressableLoad.Completed += OnLoadDone;
    }
    IEnumerator OnLoad() {
        while(addressableLoad.PercentComplete <1f) {
            loading.text = addressableLoad.PercentComplete.ToString("P");
            print($"{addressableLoad.PercentComplete.ToString("P")}");
            yield return null;
        }
    }

    private void OnLoadDone(AsyncOperationHandle<Material> mat) {
        StartCoroutine(ReleaseAddessableAssets());
        _skybox.material = mat.Result;
        index++;
        ToReleaseMemory(isUnloadUnusedAssets.isOn, isGCCollect.isOn);
    }

    IEnumerator ReleaseAddessableAssets() {
        // if(addressableLoad.IsDone && isUnloadRelease.isOn)Addressables.Release(addressableLoad);
        if(_skybox.material != null && isUnloadRelease.isOn) {
            onGUIText += "\n Do Addressables.Release(_skybox.material)";
            Addressables.Release(_skybox.material);
        }
        yield return new WaitForSeconds(0);
    }

    IEnumerator wait2second() {
        yield return new WaitForSeconds(3);
    }
    
    public void NewInstanceFromResources() {
        var prefabResource = Resources.Load<GameObject>("SphereResource");
        var instance = Instantiate(prefabResource,this.transform);
        instance.transform.localPosition = new Vector3(transform.childCount *2f, 0, 0);
        instance.GetComponent<MeshRenderer>().materials[0].color = Color.green;
    }

    public void DestroyInstance() {
        for(var i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        ToReleaseMemory(isUnloadUnusedAssets, isGCCollect);
    }

    public void ToReleaseMemory(bool isUnloadUnusedAssets, bool isGCCollect) {
        if(isUnloadUnusedAssets) {
            onGUIText += "\nDO Resources.UnloadUnusedAssets()";
            Resources.UnloadUnusedAssets();
        }
        if(isGCCollect) {
            onGUIText += "\nDO GC.Collect()";
            GC.Collect();
        }
    }
}
