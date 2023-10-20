# colordiff

A simple console program to test the performance of RGB color similarity by conversion to [OKLab](https://bottosson.github.io/posts/oklab/) space. This optionally uses a Halley iterator to replace the costly cube-root calculations. One million loops on my machine takes about 210ms for true cube-roots, and about 160ms for the Halley method. (Of course, that's serial CPU execution and not parallel GPU execution.)

A visual representation I posted [here](https://www.shadertoy.com/view/cdcBDs) on Shadertoy (click the "play" button):

<iframe width="640" height="360" frameborder="0" src="https://www.shadertoy.com/embed/cdcBDs?gui=true&t=10&paused=true&muted=false" allowfullscreen></iframe>

Usage:

```
colordiff r1 g1 b1 r2 g2 b2 [loops]
```

If you don't specify `loops` it outputs the "distance" as a normalzed (0-1) value as well as the RGB and OKLab values.

Sample output:

```
> colordiff 75 200 110 90 105 100
Distance: 0.15555418
  color1 Lab (0.8554384000, -0.0992842000, 0.0471218200)
  color1 RGB (0.2941177000, 0.7843137000, 0.4313726000)
  color2 Lab (0.7336869000, -0.0139876900, 0.0013160710)
  color2 RGB (0.3529412000, 0.4117647000, 0.3921569000)

> colordiff 0 0 0 255 255 255 1000000
Elapsed for real cube-root  209ms
Elapsed for Halley iterator 160ms
```

Although the Newton method is more well-known than the Halley method, it has a higher error versus true cube root for the same number of iterations. My testing shows the error rate is approximately the same with twice as many loops, which brings the one-million-loop time to within about 10ms of the built-in .NET cube root function.

