using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

public class SystemChannelSample : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        await Single();
        await SingleToMulti();
        await MultiToSingle();
    }


    async Task Single()
    {
        var channel = Channel.CreateUnbounded<int>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });

        var consumer = Task.Run(async () =>
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                Debug.Log(await channel.Reader.ReadAsync());
            }
        });

        var producer = Task.Run(async () =>
        {
            await channel.Writer.WriteAsync(1);
            await channel.Writer.WriteAsync(2);
            await channel.Writer.WriteAsync(3);
            channel.Writer.Complete();
        });

        await Task.WhenAll(consumer, producer);

        Debug.Log("Completed.");
    }

    async Task SingleToMulti()
    {
        var channel = Channel.CreateUnbounded<int>(
            new UnboundedChannelOptions
            {
                SingleWriter = true,
            });

        var consumers = Enumerable.Range(1, 3)
            .Select(consumerId =>
                Task.Run(async () =>
                {
                    while (await channel.Reader.WaitToReadAsync())
                    {
                        if (channel.Reader.TryRead(out var value))
                        {
                            Debug.Log($"Consumer{consumerId}:{value}");
                        }
                    }
                }));

        var producer = Task.Run(async () =>
        {
            await channel.Writer.WriteAsync(1);
            await channel.Writer.WriteAsync(2);
            await channel.Writer.WriteAsync(3);
            channel.Writer.Complete();
        });

        await Task.WhenAll(consumers.Union(new[] { producer }));

        Debug.Log("Completed.");
    }

    async Task MultiToSingle()
    {
        var channel = Channel.CreateUnbounded<int>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
            });

        var consumer = Task.Run(async () =>
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                Debug.Log("Producer" + await channel.Reader.ReadAsync());
            }
        });

        var producers = Enumerable.Range(1, 3)
            .Select(producerId =>
                Task.Run(async () =>
                {
                    await channel.Writer.WriteAsync(producerId);
                }));

        await Task.WhenAll(producers);
        channel.Writer.Complete();

        await consumer;

        Debug.Log("Completed.");
    }
}
