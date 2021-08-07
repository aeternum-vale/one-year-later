
using System.Threading;
using Cysharp.Threading.Tasks;

namespace OneYearLater.UI.Interfaces
{
	public interface IAsyncFadable
	{
		UniTask FadeAsync();
		UniTask UnfadeAsync();
		UniTask FadeAsync(CancellationToken token);
		UniTask UnfadeAsync(CancellationToken token);
	}

}