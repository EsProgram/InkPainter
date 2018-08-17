
<p align="center">
  <img src="https://github.com/EsProgram/InkPainterDocument/blob/master/UnityTexturePaint_Icons/logo.png" width="600"/>
</p>

------------

# InkPainter

[![GitHub license](https://img.shields.io/github/license/EsProgram/InkPainter.svg)](https://github.com/EsProgram/InkPainter/blob/master/LICENSE.txt)
[![release](https://img.shields.io/badge/release-nv1.2.1-blue.svg)](https://github.com/EsProgram/InkPainter/releases)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/EsProgram/InkPainter/pulls)


[![GitHub issues](https://img.shields.io/github/issues/EsProgram/InkPainter.svg)](https://github.com/EsProgram/InkPainter/issues)
[![GitHub forks](https://img.shields.io/github/forks/EsProgram/InkPainter.svg)](https://github.com/EsProgram/InkPainter/network)
[![GitHub stars](https://img.shields.io/github/stars/EsProgram/InkPainter.svg)](https://github.com/EsProgram/InkPainter/stargazers)
[![Twitter](https://img.shields.io/twitter/url/https/github.com/EsProgram/InkPainter.svg?style=social)](https://twitter.com/intent/tweet?text=Wow:&url=https%3A%2F%2Fgithub.com%2FEsProgram%2FInkPainter)



This asset allows you to Texture-Paint on Unity.
Selling at [Asset Store](https://www.assetstore.unity3d.com/jp/#!/content/86210).

Document is [here](https://esprogram.github.io/InkPainterDocument/).


## How to use

Attach a "InkCanvas" to the object you want to paint and call the Paint method from any script.

ex)
```SamplePainter.cs
using Es.InkPainter;

public class SamplePainter : MonoBehaviour
{
	[SerializeField]
	private Brush brush;

	private void Update()
	{
		if(Input.GetMouseButton(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo))
			{
				var paintObject = hitInfo.transform.GetComponent<InkCanvas>();
				if(paintObject != null)
					paintObject.Paint(brush, hitInfo);
			}
		}
	}
}
```
See documentation and movies for more details.

## Extended Asset

It is an asset of another repository using InkPainter.

* [WaveformProvider](https://github.com/EsProgram/WaveformProvider)

<p align="center">
  <a href="https://github.com/EsProgram/WaveformProvider">
   <img src="https://github.com/EsProgram/WaveformProvider/blob/master/Image/002.gif" width="600"/>
  </a>
</p>
<p align="center">
  <a href="https://github.com/EsProgram/WaveformProvider">
   <img src="https://github.com/EsProgram/WaveformProvider/blob/master/Image/005.gif" width="600"/>
  </a>
</p>


## Movies

<p align="center">
  <img src="https://github.com/EsProgram/InkPainter/blob/master/Capture/drop.gif" width="600"/>
</p>
<p align="center">
  Liquid paint affected by normal map.
</p>

<p align="center">
  <a href="https://www.youtube.com/watch?v=TWGK5UQ6KsU">
   <img src="http://img.youtube.com/vi/TWGK5UQ6KsU/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter demo movie(Click image).
</p>

<br/>
<p align="center">
  <a href="https://youtu.be/rsH0279pIoU?list=PLemdDkL7bE3IOmxNyz07uA3mydSbXGouY">
   <img src="http://img.youtube.com/vi/rsH0279pIoU/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter rainy day camera effect(Click image).
</p>

<br/>
<p align="center">
  <a href="https://www.youtube.com/watch?v=i6Ecml0CwLU">
   <img src="http://img.youtube.com/vi/i6Ecml0CwLU/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter fluid paint setup(Click image).
</p>

<br/>
<p align="center">
  <a href="https://www.youtube.com/watch?v=3wI5HWzeJgs">
   <img src="http://img.youtube.com/vi/3wI5HWzeJgs/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter simple paint setup(Click image).
</p>

<br/>
<p align="center">
  <a href="https://www.youtube.com/watch?v=ICETzPk8Jic">
   <img src="http://img.youtube.com/vi/ICETzPk8Jic/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter setup(Click image).
</p>

<br/>
<p align="center">
  <a href="https://www.youtube.com/watch?v=Se_RuI2rl2M">
   <img src="http://img.youtube.com/vi/Se_RuI2rl2M/0.jpg" width="600"/>
  </a>
</p>
<p align="center">
 InkPainter how to use(Click image).
</p>

## Sponsor

* <a href="http://jirka.marsik.me/">Jirka Maršík</a>
* <a href="https://www.twitter.com/daxpandhi">Dax Pandhi</a>

# The MIT License

Copyright (c) 2017 Es_Program

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
