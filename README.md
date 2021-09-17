<!--
  This file is part of Flood, an interactive flood-fill visualizer.

  Copyright (C) 2021 Eliah Kagan <degeneracypressure@gmail.com>

  Permission to use, copy, modify, and/or distribute this software for any
  purpose with or without fee is hereby granted.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
  REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
  AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
  INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
  LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
  OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
  PERFORMANCE OF THIS SOFTWARE.
-->

# Flood - interactive flood-fill visualizer

Flood animates [flood fills](https://en.wikipedia.org/wiki/Flood_fill) on a
canvas interactively, allowing the user to set each fill&rsquo;s speed, data
structure used to store coordinates of pixels that have been &ldquo;seen&rdquo;
but have not yet been filled, and order in which neighboring pixels are
enumerated and added to that data structure.

Several choices of data structure are provided, including the usual options of
a stack (FIFO) or queue (LIFO). A recursively implemented flood fill is also
provided, in such a way that (like the iteratively implemented approaches) it
does not overflow the call stack even when used to fill a large region.

In effect, Flood is a rudimentary raster graphics editing canvas, whose
&ldquo;bucket tool&rdquo; operates slowly and without blocking other edits to
the canvas. As a flood fill proceeds, you can continue editing the canvas,
interacting with the fill&mdash;including by starting additional concurrent
flood fills.

**This is alpha 6 of Flood.** The program is not yet production quality, and it
has some major bugs. See [`BUGS.md`](BUGS.md).

See also
[TreeTraversalAnimations](https://github.com/EliahKagan/TreeTraversalAnimations).

*Note: Flood&rsquo;s built-in help retrieves fonts and JavaScript libraries
from the internet every time it runs, even though Flood is not itself a webapp.
This was convenient for development, but I do not regard it to be a good
software engineering practice for a desktop application. So I consider this
behavior a bug that must be fixed before putting out a non-alpha release of
Flood. One would typically never notice this behavior, at least when running
Flood with an active internet connection, since CDNs are very fast. But if it
seems like Flood is &ldquo;phoning home&rdquo; while running, that&rsquo;s
what&rsquo;s going on.*

## How to Use

Flood is a Windows program, implemented as a
[LINQPad](https://www.linqpad.net/) query. To run Flood, clone this repository,
then open `flood.linq` in LINQPad 6.

The program&rsquo;s user interface is unfortunately not very intuitive. This is
partly because it is an alpha version of the program; partly to do with its
origin as a prototype for what I intended to be a separate, portable
application with a different interface, which I still do hope also to write;
and partly to do what I *personally* find intuitive and convenient, which is
far from universal.

This README file provides only condensed documentation. For more documentation,
including detailed usage guidance and implementation notes, please see the full
help, `doc/help.html`, which can also be viewed in Flood&rsquo;s built-in help
browser [or visited online](https://eliahkagan.github.io/Flood/doc/help.html).

When running Flood, you can also click the *Show Tips* button to open brief
help, which I recommend. The tips are presented in condensed, tabular form.
Hovering your mouse cursor over an item in the tips shows a tooltip with more
information, and clicking on an item opens (or switches to) the built-in help
browser, scrolling to the relevant section of the help and highlighting the
specific relevant part.

The tips can also be browsed by going to `doc/tips.html` in a web browser,
locally [or on the web](https://eliahkagan.github.io/Flood/doc/tips.html). When
viewed that way, the items behave as ordinary hyperlinks into the full help.

## License

Flood is free software. It is licensed under
[0BSD](https://spdx.org/licenses/0BSD.html) (the &ldquo;Zero-Clause BSD
License,&rdquo; also called the [Free Public License
1.0.0](https://opensource.org/licenses/0BSD)). This license is very permissive;
it is said to be [&ldquo;public-domain
equivalent.&rdquo;](https://en.wikipedia.org/wiki/Public-domain-equivalent_license)

**See [`LICENSE`](LICENSE).**

## Dependencies

Flood&rsquo;s dependencies (other than LINQPad) are also free as in freedom,
but they are offered under other licenses. Some of them are retrieved
automatically via [NuGet](https://www.nuget.org/) and cached; LINQPad will
prompt you to do this unless you happen to already have those libraries. Others
are downloaded automatically from
[CDNs](https://en.wikipedia.org/wiki/Content_delivery_network) (though in a
future version I intend to bundle them instead).

**See [`NOTICES.md`](NOTICES.md)**, or the Dependencies section in
`doc/help.html`, for a list of dependencies, with authorship and
copyright/licensing information.

## Acknowledgements

I&rsquo;d like to thank:

- David Vassallo, for testing the program and giving usability feedback.
- Thomas Fallon, for testing the program and giving usability feedback, and
  finding a severe performance bug.
- [Zanna Star](https://github.com/ZannaStar), for examining and giving advice
  about how to improve the presentation and styling for `help.html`. It was
  rather difficult to read before, and it is much better as a result of her
  suggestions.
- Finlay McWalter, who [created static flood-fill
  animations](https://en.wikipedia.org/w/index.php?title=Talk:Flood_fill&oldid=804243376#Large_scale_animation)
  for [&ldquo;Flood fill&rdquo; on English
  Wikipedia](https://en.wikipedia.org/wiki/Flood_fill#Moving_the_recursion_into_a_data_structure),
  demonstrating the effect of a stack vs. a queue, and other Wikipedians who
  have contributed to that article. McWalter&rsquo;s animations were what
  [inspired me to write](#Independent) this program.
- The authors of the libraries and fonts this program uses, listed in
  [`NOTICES.md`](NOTICES.md).

## Goals

The main goals of Flood (besides being fun and [looking cool](#Interfering))
are to demonstrate two concepts in computer science:

1. The flood fill algorithm: how it is really a graph traversal problem, and
   the way it is affected by choice of data structure and other related
   customizations.

2. Asynchronous programming in the context of responsive user-interface
   interaction, and the way language-level features for asynchronous
   programming (like `async`/`await` in C#, which this program uses) facilitate
   code whose structure clearly reflects the problem being solved, much more so
   than if one were to code up a state machine manually.

The main reason I&rsquo;ve written Flood a a LINQPad query is to make it easy
to look at the source code while running the program, and to experiment by
making changes to the source code re-running the program to see the difference.
LINQPad displays Flood&rsquo;s source code and its user interface side-by-side.

## Reading the Code

Although ease of demonstration is sometimes a justification for cramming more
source code into a single file than one otherwise would, this is currently
excessive: the main source code file, `flood.linq`, is over 3500 lines. I
regard this situation to be a bug and I intend to split that file into several
files and/or make the code more easy to navigate in other ways. (LINQPad fully
supports queries spanning multiple files, so that&rsquo;s not the issue.)

The most interesting part of the code is the part that implements iterative
flood fill as an asynchronous method. This is the `FloodFillAsync` method in
the `MainPanel` class. See also the `RecursiveFloodFillAsync` method, which
appears immediately after `FloodFillAsync`.

## Non-Interacting and Interacting Fills

The kinds of animations you see, and what they reveal, depend on how you choose
to use Flood. Two opposite patterns are of particular interest:

### Independent

If you do at most one fill per region at a time, with only a stack or queue,
and with only fixed-order neighbor enumeration, and you do not otherwise modify
the canvas, the results resemble the static animations shown in the [Moving the
recursion into a data
structure](https://en.wikipedia.org/wiki/Flood_fill#Moving_the_recursion_into_a_data_structure)
section of [&ldquo;Flood fill&rdquo; (English
Wikipedia)](https://en.wikipedia.org/wiki/Flood_fill).

I am grateful to Finlay McWalter, who [made those
animations](https://en.wikipedia.org/w/index.php?title=Talk:Flood_fill&oldid=804243376#Large_scale_animation),
which inspired me to write this program. (I did not refer to McWalter&rsquo;s
source code in writing this, however.)

### Interfering

When multiple concurrent flood fills are *nested*&mdash;with some working to
fill a region that others are trying to expand&mdash;they can interact and
interfere (&ldquo;fight&rdquo;) with each other. This is rarely relevant to
practical applications of flood fills, since flood fills are usually carried
out atomically rather than having their steps interleaved with other
operations. But it can produce visually interesting patterns that take a long
time to terminate, particularly when several (or perhaps many) different fills,
of different kinds, interact.

I hadn&rsquo;t anticipated that when I started working on this program, but I
now consider it to be the most interesting use of the program, or at least the
most fun.
