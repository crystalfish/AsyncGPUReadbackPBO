using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class AsyncCapture : MonoBehaviour
{
    //IEnumerator Start()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1);
    //        yield return new WaitForEndOfFrame();

    //        var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
    //        ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
    //        AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);
    //        RenderTexture.ReleaseTemporary(rt);
    //    }
    //}
    
    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }
        if (ResponeseIndex == 100)
        {
            Debug.Log("GPU readback error detected. over " + ResponeseIndex);
            return;
        }
        ResponseTimeList[ResponeseIndex].Stop();
        Debug.LogError("当前帧" + ResponeseIndex + "ResponseTime time :" + ResponseTimeList[ResponeseIndex].ElapsedTicks / 10000.0f + "ms");
        var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        tex.LoadRawTextureData(request.GetData<uint>());
        tex.Apply();
        File.WriteAllBytes(@"D:\test\test" + ResponeseIndex + ".png", ImageConversion.EncodeToPNG(tex));
        ResponeseIndex++;
        Destroy(tex);
    }
    private void Update()
    {
        if(!isTest)
        {
            isTest = true;
            StartCoroutine(Test());
        }
    }
    bool isTest = false;
    int curRequestIndex = 0;
    int ResponeseIndex = 0;

    System.Diagnostics.Stopwatch RequestTime = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch ResponseTime = new System.Diagnostics.Stopwatch();
    private List<System.Diagnostics.Stopwatch> ResponseTimeList = new List<System.Diagnostics.Stopwatch>();
    /// <summary>
    /// 回调方式
    /// </summary>
    /// <returns></returns>
    IEnumerator Test()
    {
        Debug.LogError("one");
        while (true)
        {
            //yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();
            
            var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            Camera.main.targetTexture = rt;
            RenderTexture.active = rt;
            //ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
            RequestTime = new System.Diagnostics.Stopwatch();
            RequestTime.Start();
            ResponseTime = new System.Diagnostics.Stopwatch();
            ResponseTimeList.Add(ResponseTime);
            ResponseTime.Start();
            AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32, OnCompleteReadback);
            RequestTime.Stop();
            Debug.LogError("当前帧"+curRequestIndex + "reqeust time :"+RequestTime.ElapsedTicks/10000.0f + "ms");
            RenderTexture.active = null;
            if (curRequestIndex == 99)
            {
                Camera.main.targetTexture = null;
               
            }
            RenderTexture.ReleaseTemporary(rt);
            curRequestIndex++;
            if (curRequestIndex == 100)
            {
                yield break;

            }
            
        }
        
    }
    IEnumerator Test1()
    {
        //Debug.LogError("one");
        while (true)
        {
            //yield return new WaitForSeconds(1);
            // yield return new WaitForEndOfFrame();

            var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
            System.Diagnostics.Stopwatch wTime = new System.Diagnostics.Stopwatch();
            wTime.Start();
            var req = AsyncGPUReadback.Request(rt, 0, TextureFormat.ARGB32);
            yield return new WaitUntil(()=> req.done);
            wTime.Stop();
            Debug.LogError("reqeust time :" + wTime.ElapsedTicks / 10000.0f + "ms");
            //var colorArray = req.GetData<Color32>().ToArray();
            
            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            tex.LoadRawTextureData(req.GetData<uint>());
            tex.Apply();
            File.WriteAllBytes(@"D:\test\test"+curRequestIndex+ ".png", ImageConversion.EncodeToPNG(tex));
            RenderTexture.ReleaseTemporary(rt);
            Destroy(tex);
           
           
            curRequestIndex++;
            Debug.LogError(curRequestIndex);
            if (curRequestIndex == 100)
            {
                yield break;
            }
            Debug.LogError("after:" + curRequestIndex);
        }
    }
}
