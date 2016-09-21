# UnityTexturePaint

テクスチャーペイントを実現するアセットです。

詳細は[こちら](http://esprog.hatenablog.com/entry/2016/06/04/145713)。

<p align="center">
  <img src="Capture/GIF.gif" width="600"/>
</p>


エフェクト盛ればこんなふうに使えます。

<p align="center">
  <img src="Capture/エフェクト付き.gif" width="600"/>
</p>

<p align="center">
  <img src="Capture/塗り塗り.gif" width="600"/>
</p>


## できること

### 塗り

任意のゲームオブジェクトに対してペイント出来ます。


<p align="center">
  <img src="Capture/通常塗り.gif" width="600"/>
</p>


### ブラシ変更

ペイントのブラシ形状を任意に変更できます。

ブラシは自作可能です。

<p align="center">
  <img src="Capture/ブラシ変更.gif" width="600"/>
</p>

### ブラシに厚みをつける

ペイントのブラシに厚みをつけることが出来ます。

バンプマップで実現しているのでこちらも自作可能です。

<p align="center">
  <img src="Capture/厚み変更.gif" width="600"/>
</p>





## 使い方


<p align="center">
  <img src="Capture/使用.gif" width="600"/>
</p>

1. 塗りたいゲームオブジェクトにDynamicCanvasコンポーネントを付ける
2. そのゲームオブジェクトについているコライダーがMeshColliderだけであることに注意する
3. 好きなタイミングでDynamicCanvas.Paintメソッドを呼ぶことでペイントできる(GIFではサンプル用のMousePainterスクリプトをカメラに付けている)
