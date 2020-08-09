using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UniTaskChannelSample : MonoBehaviour
{
    void Start()
    {
        SingleConsumer();
        Multicast();
        MultiToSingle().Forget();
    }

    private void SingleConsumer()
    {
        var channel = Channel.CreateSingleConsumerUnbounded<string>();

        var reader = channel.Reader;

        WaitForChannelAsync(reader, this.GetCancellationTokenOnDestroy()).Forget();

        var writer = channel.Writer;

        writer.TryWrite("1");
        writer.TryWrite("2");
        writer.TryWrite("3");

        writer.TryComplete();
        
        //writer.TryComplete(new Exception(""));
    }

    private async UniTaskVoid WaitForChannelAsync(ChannelReader<string> reader, CancellationToken cancellationToken)
    {
        try
        {
            var result1 = await reader.ReadAsync(cancellationToken);
            Debug.Log(result1);

            await reader.ReadAllAsync()
                .ForEachAsync(x => Debug.Log(x), cancellationToken);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void Multicast()
    {
        var channel = Channel.CreateSingleConsumerUnbounded<string>();

        var connectable = channel.Reader.ReadAllAsync().Publish();
        using (connectable.Connect())
        {
            WaitForChannelAsync(1, connectable, this.GetCancellationTokenOnDestroy()).Forget();
            WaitForChannelAsync(2, connectable, this.GetCancellationTokenOnDestroy()).Forget();
            WaitForChannelAsync(3, connectable, this.GetCancellationTokenOnDestroy()).Forget();

            var writer = channel.Writer;

            writer.TryWrite("A");
            writer.TryWrite("B");
            writer.TryWrite("C");

            writer.TryComplete();
        }
    }

    private async UniTaskVoid WaitForChannelAsync(int consumerId, IUniTaskAsyncEnumerable<string> enumerable, CancellationToken cancellationToken)
    {
        try
        {
            await enumerable.ForEachAsync(x => Debug.Log("Consumer" + consumerId + ":"  + x), cancellationToken);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async UniTaskVoid MultiToSingle()
    {
        var channel = Channel.CreateSingleConsumerUnbounded<string>();

        var reader = channel.Reader;

        WaitForChannelAsync(reader, this.GetCancellationTokenOnDestroy()).Forget();

        var writer = channel.Writer;

        var producers = Enumerable.Range(1, 3)
            .Select(producerId =>
                UniTask.Run(() =>
                {
                    writer.TryWrite("Producer" + producerId);
                }));

        await UniTask.WhenAll(producers);
        
        writer.Complete();
    }
}
