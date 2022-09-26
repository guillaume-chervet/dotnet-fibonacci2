using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Leonardo;

public record FiboData
{
    public int Input { get; set; }
    public long Output { get; set; }
    public bool IsFromCache { get; set; }
}

public  class Fibonacci
{
    
    public static int Run(int i)
    {
        if (i <= 2) return 1;
        return Run(i - 1) + Run(i - 2);
    }    
    
    public static async Task<List<long>> RunAsync(string[] args)    {          
        var tasks = new List<Task<FiboData>>();

        using var context = new FibonacciDataContext();
        foreach (var arg in args)
        {
            var int32 = Convert.ToInt32(arg);
            var queryResult = await context.TFibonaccis.Where(f => f.FibInput == int32)
                .Select(f => f.FibOutput).FirstOrDefaultAsync();

            if (queryResult == default)
            {
                var result = Task.Run(() =>
                {
                    return new FiboData
                    {
                        Output = Fibonacci.Run(int32),
                        Input = int32,
                        IsFromCache = false
                    };
                });
                tasks.Add(result);
            }
            else
            {
                tasks.Add(Task.FromResult(new FiboData
                {
                    Output = queryResult,
                    Input = int32,
                    IsFromCache = true
                }));
            }
        }

        Task.WaitAll(tasks.ToArray());
            
        var results = tasks.Select(t=>t.Result).ToList();

        foreach (var result in results)
        {
            if (!result.IsFromCache)
            {
                context.TFibonaccis.Add(new TFibonacci()
                {
                    FibOutput = result.Output,
                    FibCreatedTimestamp = DateTime.Now,
                    FibInput = result.Input,
                });
            }
        }

        await context.SaveChangesAsync();

        return results.Select(r => r.Output).ToList();
    }     
}