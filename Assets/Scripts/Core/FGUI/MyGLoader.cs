/***************************
 * Author:      luwenzhen
 * CreateTime:  9/25/2017
 * LastEditor:
 * Description:
 * 
**/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class MyGLoader : GLoader
{

    protected override void LoadExternal()
    {
        if (url.LastIndexOf("#") == -1)
        {
            AssetManager.LoadAtlas(url, OnLoadResComplete, string.Empty);
            return;
        }
        string[] arrPath = url.Split(new char[1] { '#' });
        if (arrPath.Length != 2)
        {
            Debug.LogWarning("���淶��ͼƬ·��!");
            return;
        }
        AssetManager.LoadAtlas(arrPath[0], OnLoadResComplete, arrPath[1]);
    }

    private void OnLoadResComplete(Sprite sp)
    {
        if (sp == null)
        {
            Debug.Log("load: " + url + " failed!");
            onExternalLoadFailed();
            return;
        }
        NTexture nTexture = new NTexture(sp.texture, new Rect(
            sp.textureRect.x,
            sp.texture.height - sp.textureRect.height - sp.textureRect.y,
            sp.textureRect.width,
            sp.textureRect.height));
        this.onExternalLoadSuccess(nTexture);
    }

    // �ͷŴ���
    protected override void FreeExternal(NTexture texture)
    {
        texture.refCount--;
    }

}
