This project was migrated to another approach: https://github.com/OrangeCube/MinimumAsyncBridge


# AwaitableUniRx

IAsyncMethodBuilder portability libarary on UniRx and Awaitable extensions for UniRx

※ 別の方式に移行: https://github.com/ufcpp/MinimumAsyncBridge

`TaskCompletionSource`の辺りだけ実装した最低ラインのasync/awaitバックポーティング実装を用意。
その上に、[UniRx](https://github.com/neuecc/UniRx)とか[IteratorTasks](https://github.com/OrangeCube/IteratorTasks)とかの相互運用レイヤー(UniRx/IteratorTasksのawaiter実装と`Task`への変換)を用意。

## 概要

[UniRx](https://github.com/neuecc/UniRx)  の IObservable に対して C# 5.0 の await 演算子を使えるようにするもの。

## どうやって？

### 背景: C# の機能と標準ライブラリ

C# の構文の一部はライブラリに依存していて、一定以上のバージョンの .NET Framework がないと基本的には動かない。
ただし、その必須ライブラリと全く同じシグネチャのクラス/メソッドを実装すれば、古い .NET Framework 上でも動かすことができるようになる。

C# 5.0 の非同期メソッド(async/await)もこの類の機能で、動かすためには、

- `System.Threading.Tasks.Task`
- `System.Runtime.CompilerServices.IAsyncMethodBuilder`

などのクラスやインターフェイスが必要になる。

### IAsyncMethodBuilder 実装

IAsyncMethodBuilder などの実装はそれなりに大変で、自作はかなりためらわれるものだった。
しかし、今は、[Microsoft の参照ソースコード](https://github.com/Microsoft/referencesource)が[MITライセンス](https://github.com/Microsoft/referencesource/blob/master/LICENSE.txt)で公開されているわけで、これをベースに少し書き替えれば動かせるであろう算段が付いたので、実際に実装してみた。

この部分が UniRx 依存。`Task` クラスも、UniRx の薄いラッパー。

## できること

.NET Framework 3.5 上で、UniRx と一緒にこのライブラリを参照すれば、`IObservable<T>` に対して await できるようになる。

↓これが動く。

```cs
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UniRx;

namespace SampleModels
{
    public class Class1
    {
        public async Task RunAsync(IObserver<string> observer)
        {
            var urls = new[]
            {
                "http://yahoo.co.jp",
                "http://google.co.jp",
                "http://bing.co.jp",
                "http://awsedrftgyhujikol.jp/",
            };

            foreach (var url in urls)
            {
                await Observable.Timer(TimeSpan.FromMilliseconds(10));

                try
                {
                    var res = await GetAsStringAsync(url);
                    observer.OnNext(res);
                }
                catch(Exception ex)
                {
                    observer.OnError(ex);
                }
            }
        }

        public async Task<string> GetAsStringAsync(string url)
        {
            var req = WebRequest.Create(url);
            var res = await req.GetResponseAsObservable();

            using (var sr = new StreamReader(res.GetResponseStream()))
                return sr.ReadToEnd();
        }
    }
}
```

### 制限・注意事項

このライブラリを .NET Framework 4 以上で参照すると、本家 `System.Threading.Tasks.Task` と衝突するので注意。

[Unity](http://japan.unity3d.com/unity/) からは“使えなくはない”。以下の状態。

- Unity エディター上で走るコンパイラーは C# 3.0 相当なので、結局 await は使えない
- Unity から参照する DLL 中では、await を使える

つまり、「DLL 化してから Unity 参照」という状況でだけ C# 5.0 の await 演算子を使える。

## このリポジトリの今後

このリポジトリを保守し続けるつもりはあまりない。

需要があれば、英語化した上で [UniRx](https://github.com/neuecc/UniRx) に pull-req 出す。
