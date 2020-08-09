using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sample.Channels
{
    class Program
    {
        static void Main(string[] args)
        {
            Single().Wait();
            //SingleToMulti().Wait();
            //MultiToSingle().Wait();
        }

        static async Task Single()
        {
            var channel = Channel.CreateUnbounded<int>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = true,
                });

            var consumer = Task.Run(async () =>
            {
                while(await channel.Reader.WaitToReadAsync())
                {
                    Console.WriteLine(await channel.Reader.ReadAsync());
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

            Console.WriteLine("Completed.");
        }

        static async Task SingleToMulti()
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
                                Console.WriteLine($"Consumer{consumerId}:{value}");
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

            Console.WriteLine("Completed.");
        }

        static async Task MultiToSingle()
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
                    Console.WriteLine("Producer" + await channel.Reader.ReadAsync());
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

            Console.WriteLine("Completed.");
        }
    }
}
