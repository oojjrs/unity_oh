using System.Threading.Tasks;
using UnityEngine;

public static class AsyncOperationExtensions
{
    public static Task ToTask(this AsyncOperation asyncOperation)
    {
        var tcs = new TaskCompletionSource<bool>();
        asyncOperation.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }
}
