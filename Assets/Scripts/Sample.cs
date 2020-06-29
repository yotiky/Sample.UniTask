using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;

public class Sample : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        await BasicUsageSampleAsync();
    }

    #region Basic usage
    private async UniTask BasicUsageSampleAsync()
    {
        using(var meas = new Measurement(nameof(BasicUsageUniTask)))
        {
            await BasicUsageUniTask();
        }

        using (var meas = new Measurement(nameof(BasicUsageUniTaskNotAwait)))
        {
            await BasicUsageUniTaskNotAwait();
        }

        bool result;
        using (var meas = new Measurement(nameof(BasicUsageUniTaskT)))
        {
            result = await BasicUsageUniTaskT();
        }
        Debug.Log("BasicUsageUniTaskT Result:" + result);

        using (var meas = new Measurement(nameof(BasicUsageUniTaskVoid)))
        {
            BasicUsageUniTaskVoid().Forget();
        }
    }
    // 戻り値を持たない非同期処理
    private async UniTask BasicUsageUniTask()
    {
        await UniTask.Delay(1000);
    }
    // 待つ必要がなければ await せずに UniTask を返す (コンパイルで await の展開が入ら無いので余計なオーバーヘッドを防げる)
    private UniTask BasicUsageUniTaskNotAwait()
    {
        return UniTask.Delay(1000);
    }
    // 戻り値を持つ非同期処理
    private async UniTask<bool> BasicUsageUniTaskT()
    {
        await UniTask.Delay(1000);
        return true;
    }
    // async void 相当、Forget() とセットで使う
    private async UniTaskVoid BasicUsageUniTaskVoid()
    {
        await UniTask.Delay(1000);
    }
    #endregion

}
