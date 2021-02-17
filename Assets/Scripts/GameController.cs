using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public Toggle isUnloadUnusedAssets;
    public Toggle isGCCollect;
    private Skybox _skybox;
    private AssetBundle assetBundle;
    private int index;
    private string skyboxName = "skybox";

    private void Start() {
        _skybox = Camera.main.GetComponent<Skybox>();
        
        // assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,"skybox0"));
        //
        // if (assetBundle == null) {
        //     Debug.Log("Failed to load AssetBundle!");
        //     return;
        // }
        //
        // //load the first skybox
        // var material = assetBundle.LoadAsset<Material>(skyboxName + "0");
        // _skybox.material = material;
    }
    
    public void ToNextSkyBoxBySteamingAssets() {
        if(assetBundle != null) {
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
    
    
    public void ToNextSkyBox() {
        if(index > 5) index = 0;
        var path = "skybox" + index;
        var material = Resources.Load<Material>(path);
        _skybox.material = material;
        index++;
        ToReleaseMemory(isUnloadUnusedAssets.isOn, isGCCollect.isOn);
    }
    
    public void ToNextSkyBoxByAddressableAssets() {
        if(index > 5) index = 0;
        Addressables.LoadAssetAsync<Material>("skybox" + index).Completed += OnLoadDone;
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<Material> mat) {
        _skybox.material = mat.Result;
        index++;
        ToReleaseMemory(isUnloadUnusedAssets.isOn, isGCCollect.isOn);
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
            print("DO Resources.UnloadUnusedAssets()");
            Resources.UnloadUnusedAssets();
        }
        if(isGCCollect) {
            print("DO GC.Collect()");
            GC.Collect();
        }
    }
}
